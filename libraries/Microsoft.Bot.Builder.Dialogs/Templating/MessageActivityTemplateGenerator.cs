using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// The TextMessageGenerator implements IMessageActivityGenerator by using ILanguageGenerator
    /// to generate text and then uses simple markdown semantics like chatdown to create complex
    /// attachments such as herocards, image cards, image attachments etc.
    /// </summary>
    public class MessageActivityGenerator : IMessageActivityGenerator
    {
        // Fixed text constructor
        public MessageActivityGenerator()
        {
        }

        /// <summary>
        /// Generate the activity. 
        /// </summary>
        /// <param name="turnContext">turn context.</param>
        /// <param name="template">(optional) inline template definition.</param>
        /// <param name="data">data to bind the template to.</param>
        /// <returns>message activity.</returns>
        public async Task<IMessageActivity> Generate(ITurnContext turnContext, string template, object data)
        {
            object lgOriginResult = template;
            var languageGenerator = turnContext.TurnState.Get<ILanguageGenerator>();
            if (languageGenerator != null)
            {
                // data bind template 
                lgOriginResult = await languageGenerator.Generate(turnContext, template, data).ConfigureAwait(false);
            }
            else
            {
                Trace.TraceWarning($"There is no ILanguageGenerator registered in the ITurnContext so no data binding was performed for template: {template}");
            }

            // render activity from template
            return await CreateActivity(lgOriginResult).ConfigureAwait(false);
        }

        /// <summary>
        /// Given a lg result, create an activity.
        /// </summary>
        /// <remarks>
        /// This method will create an MessageActivity from text ot JToken.
        /// </remarks>
        /// <param name="lgOutput">lg output.</param>
        /// <returns>MessageActivity for it.</returns>
        public async Task<IMessageActivity> CreateActivity(object lgOutput)
        {
            var activity = Activity.CreateMessageActivity();
            activity.TextFormat = TextFormatTypes.Markdown;
            if (lgOutput is JObject lgJObj)
            {
                return CreateActivityFromJObj(lgJObj);
            }

            return CreateActivityFromText(lgOutput.ToString());
        }

        private IMessageActivity CreateActivityFromText(string text)
        {
            var activity = Activity.CreateMessageActivity();
            activity.Text = text.Trim();
            activity.Speak = text.Trim();

            return activity;
        }

        private IMessageActivity CreateActivityFromJObj(JObject lgJObj)
        {
            var activity = Activity.CreateMessageActivity();
            activity.TextFormat = TextFormatTypes.Markdown;

            if (GetAttachment(lgJObj, out var attachment))
            {
                activity.Attachments.Add(attachment);
            }
            else
            {
                var type = GetStructureType(lgJObj);

                if (type == nameof(Activity))
                {
                    BuildNormalActivity(activity, lgJObj);
                }
                else
                {
                    activity = CreateActivityFromText(lgJObj.ToString());
                }
            }

            return activity;
        }

        private void BuildNormalActivity(IMessageActivity activity, JObject lgJObj)
        {
            foreach (var item in lgJObj)
            {
                var property = item.Key.Trim().ToLower();
                var value = item.Value;

                switch (property.ToLower())
                {
                    case "text":
                        activity.Text = value.ToString();
                        break;

                    case "speak":
                        activity.Speak = value.ToString();
                        break;

                    case "inputhint":
                        activity.InputHint = value.ToString();
                        break;

                    case "attachments":
                        activity.Attachments = GetAttachments(value);
                        break;

                    case "suggestedactions":
                        activity.SuggestedActions = GetSuggestions(value);
                        break;

                    case "attachmentlayout":
                        activity.AttachmentLayout = value.ToString();
                        break;

                    default:
                        Debug.WriteLine(string.Format("Skipping unknown activity property {0}", property));
                        break;
                }
            }
        }

        private SuggestedActions GetSuggestions(JToken value)
        {
            var suggestions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
            };

            var actions = FlattenValue(value);

            foreach (var action in actions)
            {
                if (action is JValue jValue && jValue.Type == JTokenType.String)
                {
                    var actionStr = jValue.ToObject<string>().Trim();
                    suggestions.Actions.Add(new CardAction(type: ActionTypes.MessageBack, title: actionStr, displayText: actionStr, text: actionStr));
                }
                else if (action is JObject actionJObj && GetCardAction(actionJObj, out var cardAction))
                {
                    suggestions.Actions.Add(cardAction);
                }
            }

            return suggestions;
        }

        private List<CardAction> GetButtons(JToken value)
        {
            var buttons = new List<CardAction>();
            var actions = FlattenValue(value);

            foreach (var action in actions)
            {
                if (action is JValue jValue && jValue.Type == JTokenType.String)
                {
                    var actionStr = jValue.ToObject<string>().Trim();
                    buttons.Add(new CardAction(type: ActionTypes.ImBack, title: actionStr, value: actionStr));
                }
                else if (action is JObject actionJObj && GetCardAction(actionJObj, out var cardAction))
                {
                    buttons.Add(cardAction);
                }
            }

            return buttons;
        }

        private bool GetCardAction(JObject cardActionJObj, out CardAction cardAction)
        {
            var type = GetStructureType(cardActionJObj);
            cardAction = new CardAction();
            var isCardAction = true;
            if (type == nameof(CardAction))
            {
                foreach (var item in cardActionJObj)
                {
                    var property = item.Key.Trim().ToLower();
                    var value = item.Value;

                    switch (property.ToLower())
                    {
                        case "type":
                            cardAction.Type = value.ToString();
                            break;

                        case "title":
                            cardAction.Title = value.ToString();
                            break;

                        case "value":
                            cardAction.Value = value.ToString();
                            break;

                        case "displaytext":
                            cardAction.DisplayText = value.ToString();
                            break;

                        case "text":
                            cardAction.Text = value.ToString();
                            break;

                        case "image":
                            cardAction.Image = value.ToString();
                            break;

                        default:
                            Debug.WriteLine(string.Format("Skipping unknown activity property {0}", property));
                            break;
                    }
                }
            }
            else
            {
                isCardAction = false;
            }

            return isCardAction;
        }

        private string GetStructureType(JObject jObj)
        {
            if (jObj == null)
            {
                return string.Empty;
            }

            var type = jObj["$type"]?.ToString()?.Trim();
            if (string.IsNullOrEmpty(type))
            {
                // Adaptive card type
                type = jObj["type"]?.ToString()?.Trim();
            }

            return type ?? string.Empty;
        }

        private List<Attachment> GetAttachments(JToken value)
        {
            var attachments = new List<Attachment>();
            var attachmentsJsonList = FlattenValue(value);

            foreach (var attachmentsJson in attachmentsJsonList)
            {
                if (GetAttachment((JObject)attachmentsJson, out var attachment))
                {
                    attachments.Add(attachment);
                }
            }

            return attachments;
        }

        private bool GetAttachment(JObject lgJObj, out Attachment attachment)
        {
            attachment = new Attachment();
            var isAttachment = true;

            var type = GetStructureType(lgJObj);

            switch (type)
            {
                case nameof(HeroCard):
                    attachment = GetCardAtttachment(HeroCard.ContentType, lgJObj);
                    break;

                case nameof(ThumbnailCard):
                    attachment = GetCardAtttachment(ThumbnailCard.ContentType, lgJObj);
                    break;

                case nameof(AudioCard):
                    attachment = GetCardAtttachment(AudioCard.ContentType, lgJObj);
                    break;

                case nameof(VideoCard):
                    attachment = GetCardAtttachment(VideoCard.ContentType, lgJObj);
                    break;

                case nameof(AnimationCard):
                    attachment = GetCardAtttachment(AnimationCard.ContentType, lgJObj);
                    break;

                case nameof(SigninCard):
                    attachment = GetCardAtttachment(SigninCard.ContentType, lgJObj);
                    break;

                case nameof(OAuthCard):
                    attachment = GetCardAtttachment(OAuthCard.ContentType, lgJObj);
                    break;

                case nameof(ReceiptCard):
                    attachment = GetCardAtttachment(ReceiptCard.ContentType, lgJObj);
                    break;

                case "AdaptiveCard":
                    attachment = new Attachment("application/vnd.microsoft.card.adaptive", content: lgJObj);
                    break;

                default:
                    isAttachment = false;
                    break;
            }

            return isAttachment;
        }

        private Attachment GetCardAtttachment(string type, JObject lgJObj)
        {
            var attachment = new Attachment(type, content: new JObject());
            BuildGenericCard(attachment.Content, type, lgJObj);
            return attachment;
        }

        private void BuildGenericCard(dynamic card, string type, JObject lgJObj)
        {
            foreach (var item in lgJObj)
            {
                var property = item.Key.Trim().ToLower();
                var value = item.Value;

                switch (property.ToLower())
                {
                    case "title":
                    case "subtitle":
                    case "text":
                    case "aspect":
                    case "value":
                    case "connectionName":
                        card[property] = value;
                        break;

                    case "image":
                    case "images":
                        if (type == HeroCard.ContentType || type == ThumbnailCard.ContentType)
                        {
                            // then it's images
                            if (card["images"] == null)
                            {
                                card["images"] = new JArray();
                            }

                            var imageList = FlattenValue(value).Select(u => u.ToString()).ToList();
                            imageList.ForEach(u => ((JArray)card["images"]).Add(new JObject() { { "url", u } }));
                        }
                        else
                        {
                            // then it's image
                            var urlObj = new JObject() { { "url", value.ToString() } };
                            card["image"] = urlObj;
                        }

                        break;

                    case "media":
                        if (card[property] == null)
                        {
                            card[property] = new JArray();
                        }

                        var mediaList = FlattenValue(value).Select(u => u.ToString()).ToList();

                        mediaList.ForEach(u => ((JArray)card[property]).Add(new JObject() { { "url", u } }));
                        break;

                    case "buttons":
                        card[property] = JArray.FromObject(GetButtons(value));
                        break;

                    case "autostart":
                    case "sharable":
                    case "autoloop":
                        card[property] = value.ToString().ToLower() == "true";
                        break;
                    case "":
                        break;
                    default:
                        Debug.WriteLine(string.Format("Skipping unknown card property {0}", property));
                        break;
                }
            }
        }

        private List<JToken> FlattenValue(JToken item)
        {
            var list = new List<JToken>();
            if (item is JArray array)
            {
                list = array.ToList();
            }
            else
            {
                list.Add(item);
            }

            return list;
        }
    }
}
