﻿// Copyright (c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Schema;
using Thrzn41.WebexTeams.Version1;

[assembly: InternalsVisibleTo("Microsoft.Bot.Builder.Adapters.Webex.Tests")]

namespace Microsoft.Bot.Builder.Adapters.Webex
{
    internal static class WebexHelper
    {
        /// <summary>
        /// Gets or sets the identity of the bot.
        /// </summary>
        /// <value>
        /// The identity of the bot.
        /// </value>
        public static Person Identity { get; set; }

        /// <summary>
        /// Validates the local secret against the one obtained from the request header.
        /// </summary>
        /// <param name="secret">The local stored secret.</param>
        /// <param name="request">The <see cref="HttpRequest"/> with the signature.</param>
        /// <param name="json">The serialized payload to be use for comparison.</param>
        /// <returns>The result of the comparison between the signature in the request and hashed json.</returns>
        public static bool ValidateSignature(string secret, HttpRequest request, string json)
        {
            var signature = request.Headers.ContainsKey("x-spark-signature")
                ? request.Headers["x-spark-signature"].ToString().ToUpperInvariant()
                : throw new Exception("HttpRequest is missing \"x-spark-signature\"");

            #pragma warning disable CA5350 // Webex API uses SHA1 as cryptographic algorithm.
            using (var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(secret)))
            {
                var hashArray = hmac.ComputeHash(Encoding.UTF8.GetBytes(json));
                var hash = BitConverter.ToString(hashArray).Replace("-", string.Empty).ToUpperInvariant();

                return signature == hash;
            }
            #pragma warning restore CA5350 // Webex API uses SHA1 as cryptographic algorithm.
        }

        /// <summary>
        /// Creates a <see cref="Activity"/> using the body of a request.
        /// </summary>
        /// <param name="payload">The payload obtained from the body of the request.</param>
        /// <returns>An <see cref="Activity"/> object.</returns>
        public static Activity PayloadToActivity(WebhookEventData payload)
        {
            if (payload == null)
            {
                return null;
            }

            var activity = new Activity
            {
                Id = payload.Id,
                Timestamp = new DateTime(),
                ChannelId = "webex",
                Conversation = new ConversationAccount
                {
                    Id = payload.MessageData.SpaceId,
                },
                From = new ChannelAccount
                {
                    Id = payload.ActorId,
                },
                Recipient = new ChannelAccount
                {
                    Id = Identity.Id,
                },
                ChannelData = payload,
                Type = ActivityTypes.Event,
            };

            if (payload.MessageData.FileCount > 0)
            {
                activity.Attachments = HandleMessageAttachments(payload.MessageData);
            }

            return activity;
        }

        /// <summary>
        /// Gets a decrypted message by its Id.
        /// </summary>
        /// <param name="payload">The payload obtained from the body of the request.</param>
        /// <param name="decrypterFunc">The function used to decrypt the message.</param>
        /// <returns>A <see cref="Message"/> object.</returns>
        public static async Task<Message> GetDecryptedMessageAsync(WebhookEventData payload, Func<string, CancellationToken?, Task<Message>> decrypterFunc)
        {
            if (payload == null)
            {
                return null;
            }

            return await decrypterFunc(payload.MessageData.Id, null).ConfigureAwait(false);
        }

        /// <summary>
        /// Converts a decrypted <see cref="Message"/> into an <see cref="Activity"/>.
        /// </summary>
        /// <param name="decryptedMessage">The decrypted message obtained from the body of the request.</param>
        /// <returns>An <see cref="Activity"/> object.</returns>
        public static Activity DecryptedMessageToActivity(Message decryptedMessage)
        {
            if (decryptedMessage == null)
            {
                return null;
            }

            var activity = new Activity
            {
                Id = decryptedMessage.Id,
                Timestamp = new DateTime(),
                ChannelId = "webex",
                Conversation = new ConversationAccount
                {
                    Id = decryptedMessage.SpaceId,
                },
                From = new ChannelAccount
                {
                    Id = decryptedMessage.PersonId,
                    Name = decryptedMessage.PersonEmail,
                },
                Recipient = new ChannelAccount
                {
                    Id = Identity.Id,
                },
                Text = !string.IsNullOrEmpty(decryptedMessage.Text) ? decryptedMessage.Text : string.Empty,
                ChannelData = decryptedMessage,
                Type = ActivityTypes.Message,
            };

            // this is the bot speaking
            if (activity.From.Id == Identity.Id)
            {
                activity.Type = ActivityTypes.Event;
            }

            if (decryptedMessage.HasHtml)
            {
                var pattern = new Regex($"^(<p>)?<spark-mention .*?data-object-id=\"{Identity.Id}\".*?>.*?</spark-mention>");
                if (!decryptedMessage.Html.Equals(pattern))
                {
                    // this should look like ciscospark://us/PEOPLE/<id string>
                    var match = Regex.Match(Identity.Id, "/ciscospark://.*/(.*)/im");
                    if (match.Captures.Count > 0)
                    {
                        pattern = new Regex(
                            $"^(<p>)?<spark-mention .*?data-object-id=\"{match.Captures[1]}\".*?>.*?</spark-mention>");
                    }
                }

                var action = decryptedMessage.Html.Replace(pattern.ToString(), string.Empty);

                // Strip the remaining HTML tags and replace the message text with the the HTML version
                activity.Text = action.Replace("/<.*?>/img", string.Empty).Trim();
            }
            else
            {
                var pattern = new Regex("^" + Identity.DisplayName + "\\s+");
                activity.Text = activity.Text.Replace(pattern.ToString(), string.Empty);
            }

            if (decryptedMessage.FileCount > 0)
            {
                activity.Attachments = HandleMessageAttachments(decryptedMessage);
            }

            return activity;
        }

        /// <summary>
        /// Adds the message's files to a attachments list.
        /// </summary>
        /// <param name="message">The message with the files to process.</param>
        /// <returns>A list of attachments containing the message's files.</returns>
        public static List<Attachment> HandleMessageAttachments(Message message)
        {
            var attachmentsList = new List<Attachment>();

            for (var i = 0; i < message.FileCount; i++)
            {
                var attachment = new Attachment
                {
                    ContentUrl = message.FileUris[i].AbsoluteUri,
                };

                attachmentsList.Add(attachment);
            }

            if (attachmentsList.Count > 1)
            {
                throw new Exception("Currently Webex API takes only one attachment");
            }

            return attachmentsList;
        }
    }
}