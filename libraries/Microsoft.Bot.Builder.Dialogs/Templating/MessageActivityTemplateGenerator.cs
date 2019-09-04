using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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
            return await CreateActivity(lgOriginResult, data, turnContext, languageGenerator).ConfigureAwait(false);
        }

        /// <summary>
        /// Given a lg result, create an activity.
        /// </summary>
        /// <remarks>
        /// This method will create an MessageActivity from text ot JToken
        /// </remarks>
        /// <param name="lgOutput">lg output.</param>
        /// <param name="data">data to bind to.</param>
        /// <param name="turnContext">turnContext.</param>
        /// <param name="languageGenerator">languageGenerator.</param>
        /// <returns>MessageActivity for it.</returns>
        public async Task<IMessageActivity> CreateActivity(object lgOutput, object data, ITurnContext turnContext, ILanguageGenerator languageGenerator)
        {
            var activity = Activity.CreateMessageActivity();
            activity.TextFormat = TextFormatTypes.Markdown;
            if (lgOutput is JObject lgJObj)
            {
                return CreateActivityFromJObj(activity, lgJObj);
            }

            return CreateActivityFromText(activity, lgOutput.ToString());
        }

        private IMessageActivity CreateActivityFromText(IMessageActivity activity, string text)
        {
            activity.Text = text.Trim();
            activity.Speak = text.Trim();
            return activity;
        }

        private IMessageActivity CreateActivityFromJObj(IMessageActivity activity, JObject lgJObj)
        {
            var type = GetTemplateType(lgJObj);

            switch (type)
            {
                case "Herocard":
                    AddGenericCardAtttachment(activity, HeroCard.ContentType, lgJObj);
                    break;

                case "ThumbnailCard":
                    AddGenericCardAtttachment(activity, ThumbnailCard.ContentType, lgJObj);
                    break;

                case "AudioCard":
                    AddGenericCardAtttachment(activity, AudioCard.ContentType, lgJObj);
                    break;

                case "VideoCard":
                    AddGenericCardAtttachment(activity, VideoCard.ContentType, lgJObj);
                    break;

                case "AnimationCard":
                    AddGenericCardAtttachment(activity, AnimationCard.ContentType, lgJObj);
                    break;

                case "SigninCard":
                    AddGenericCardAtttachment(activity, SigninCard.ContentType, lgJObj);
                    break;

                case "OAuthCard":
                    AddGenericCardAtttachment(activity, OAuthCard.ContentType, lgJObj);
                    break;

                case "AdaptiveCard":
                    // json object
                    AddJsonAttachment(activity, "application/vnd.microsoft.card.adaptive", lgJObj);
                    break;

                case "Activity":
                    BuildNormalActivity(activity, lgJObj);
                    break;

                default:
                    activity.Text = lgJObj.ToString();
                    break;
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

                    case "suggestedActions":
                        var suggestions = GetStringListValues(value);
                        activity.SuggestedActions = new SuggestedActions
                        {
                            Actions = suggestions.Select(s => new CardAction(type: ActionTypes.MessageBack, title: s, displayText: s, text: s)).ToList()
                        };
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

        private string GetTemplateType(JObject jObj)
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
            if (value is JArray array)
            {
                attachments = array.Select(u => ConvertToAttachment(u)).Where(u => u != null).ToList();
            }
            else
            {
                attachments.Add(ConvertToAttachment(value));
            }

            return attachments;
        }

        private Attachment ConvertToAttachment(JToken attachmentJObj)
        {
            var attachment = new Attachment();

            if (!(attachmentJObj is JObject jObj))
            {
                return null;
            }

            var type = GetTemplateType(jObj);

            switch (type)
            {
                case "Herocard":
                    attachment.ContentType = HeroCard.ContentType;
                    break;

                case "ReceiptCard":
                    attachment.ContentType = ReceiptCard.ContentType;
                    break;

                case "ThumbnailCard":
                    attachment.ContentType = ThumbnailCard.ContentType;
                    break;

                case "AudioCard":
                    attachment.ContentType = AudioCard.ContentType;
                    break;

                case "VideoCard":
                    attachment.ContentType = VideoCard.ContentType;
                    break;

                case "AnimationCard":
                    attachment.ContentType = AnimationCard.ContentType;
                    break;

                case "SigninCard":
                    attachment.ContentType = SigninCard.ContentType;
                    break;

                case "AdaptiveCard":
                    attachment.ContentType = "application/vnd.microsoft.card.adaptive";
                    break;

                default:
                    attachment.ContentType = type;
                    break;
            }

            attachment.Content = jObj;
            return attachment;
        }

        private void AddJsonAttachment(IMessageActivity activity, string type, JObject lgJObj)
        {
            var attachment = new Attachment(type, content: lgJObj);
            activity.Attachments.Add(attachment);
        }

        private void AddGenericCardAtttachment(IMessageActivity activity, string type, JObject lgJObj)
        {
            var attachment = new Attachment(type, content: new JObject());
            BuildGenericCard(attachment.Content, type, lgJObj);
            activity.Attachments.Add(attachment);
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

                            var imageList = GetStringListValues(value);
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

                        var mediaList = GetStringListValues(value);
                        mediaList.ForEach(u => ((JArray)card[property]).Add(new JObject() { { "url", u } }));
                        break;

                    case "buttons":
                        if (card[property] == null)
                        {
                            card[property] = new JArray();
                        }

                        var buttonList = GetStringListValues(value);
                        buttonList.ForEach(u => ((JArray)card[property]).Add(new JObject() { { "title", u.Trim() }, { "type", "imBack" }, { "value", u.Trim() } }));

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

        private List<string> GetStringListValues(JToken item)
        {
            var list = new List<string>();
            if (item is JArray array)
            {
                list = array.Select(u => u.ToString()).ToList();
            }
            else
            {
                list.Add(item.ToString());
            }

            return list;
        }
    }
}
