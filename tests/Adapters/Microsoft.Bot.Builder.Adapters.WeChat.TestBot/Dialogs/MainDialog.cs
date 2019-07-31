﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Builder.Adapters.WeChat.TestBot
{
    public class MainDialog : ComponentDialog
    {
        protected readonly ILogger _logger;

        public MainDialog(ILogger<MainDialog> logger)
            : base(nameof(MainDialog))
        {
            _logger = logger;

            // Define the main dialog and its related components.
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(
                new WaterfallDialog(
                nameof(WaterfallDialog),
                new WaterfallStep[] { ChoiceCardStepAsync, ShowCardStepAsync }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        // 1. Prompts the user if the user is not in the middle of a dialog.
        // 2. Re-prompts the user when an invalid input is received.
        private async Task<DialogTurnResult> ChoiceCardStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("MainDialog.ChoiceCardStepAsync");

            // Create the PromptOptions which contain the prompt and re-prompt messages.
            // PromptOptions also contains the list of choices available to the user.
            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text("What card would you like to see? You can click or type the card name"),
                RetryPrompt = MessageFactory.Text("That was not a valid choice, please select a card or number from 1 to 9."),
                Choices = GetChoices(),
            };

            // Prompt the user with the configured PromptOptions.
            return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
        }

        // Send a Rich Card response to the user based on their choice.
        // This method is only called when a valid prompt response is parsed from the user's response to the ChoicePrompt.
        private async Task<DialogTurnResult> ShowCardStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("MainDialog.ShowCardStepAsync");

            // Reply to the activity we received with an activity.
            var reply = stepContext.Context.Activity.CreateReply();

            // Cards are sent as Attachments in the Bot Framework.
            // So we need to create a list of attachments on the activity.
            reply.Attachments = new List<Attachment>();

            // Decide which type of card(s) we are going to show the user
            switch (((FoundChoice)stepContext.Result).Value)
            {
                case "Animation Card":
                    // Display an AnimationCard.
                    reply.Attachments.Add(Cards.GetAnimationCard().ToAttachment());
                    break;
                case "Audio Card":
                    // Display an AudioCard
                    reply.Attachments.Add(Cards.GetAudioCard().ToAttachment());
                    break;
                case "Hero Card":
                    // Display a HeroCard.
                    reply.Attachments.Add(Cards.GetHeroCard().ToAttachment());
                    break;
                case "Receipt Card":
                    // Display a ReceiptCard.
                    reply.Attachments.Add(Cards.GetReceiptCard().ToAttachment());
                    break;
                case "Signin Card":
                    // Display a SignInCard.
                    reply.Attachments.Add(Cards.GetSigninCard().ToAttachment());
                    break;
                case "Thumbnail Card":
                    // Display a ThumbnailCard.
                    reply.Attachments.Add(Cards.GetThumbnailCard().ToAttachment());
                    break;
                case "Video Card":
                    // Display a VideoCard
                    reply.Attachments.Add(Cards.GetVideoCard().ToAttachment());
                    break;
                default:
                    // Give the user instructions about what to do next
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("Type anything to see another card."), cancellationToken);
                    return await stepContext.EndDialogAsync();
            }

            // Send the card(s) to the user as an attachment to the activity
            await stepContext.Context.SendActivityAsync(reply, cancellationToken);

            // Give the user instructions about what to do next
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Type anything to see another card."), cancellationToken);

            return await stepContext.EndDialogAsync();
        }

        private IList<Choice> GetChoices()
        {
            var cardOptions = new List<Choice>()
            {
                new Choice() { Value = "Animation Card", Synonyms = new List<string>() { "animation" } },
                new Choice() { Value = "Audio Card", Synonyms = new List<string>() { "audio" } },
                new Choice() { Value = "Hero Card", Synonyms = new List<string>() { "hero" } },
                new Choice() { Value = "Receipt Card", Synonyms = new List<string>() { "receipt" } },
                new Choice() { Value = "Signin Card", Synonyms = new List<string>() { "signin" } },
                new Choice() { Value = "Thumbnail Card", Synonyms = new List<string>() { "thumbnail", "thumb" } },
                new Choice() { Value = "Video Card", Synonyms = new List<string>() { "video" } },
            };

            return cardOptions;
        }
    }
}
