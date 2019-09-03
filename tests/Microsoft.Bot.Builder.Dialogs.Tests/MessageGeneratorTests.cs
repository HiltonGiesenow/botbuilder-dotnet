﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AdaptiveCards;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    [TestClass]
    public class MessageGeneratorTests
    {
        private static ResourceExplorer resourceExplorer;

        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            TypeFactory.Configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
            TypeFactory.RegisterAdaptiveTypes();
            resourceExplorer = ResourceExplorer.LoadProject(GetProjectFolder());
        }

        [TestMethod]
        public async Task TestInline()
        {
            var context = GetTurnContext(new MockLanguageGenerator());
            var mg = new MessageActivityGenerator();
            var activity = await mg.Generate(context, "text", data: null);
            Assert.AreEqual(ActivityTypes.Message, activity.Type);
            Assert.AreEqual("text", activity.Text);
            Assert.AreEqual("text", activity.Speak);
        }

        [TestMethod]
        public async Task TestHerocard()
        {
            var context = await GetTurnContext("NonAdaptiveCardActivity.lg");
            var mg = new MessageActivityGenerator();
            dynamic data = new JObject();
            data.type = "herocard";
            IMessageActivity activity = await mg.Generate(context, "[HeroCardTemplate]", data: data);
            Assert.AreEqual(ActivityTypes.Message, activity.Type);
            Assert.IsTrue(string.IsNullOrEmpty(activity.Text));
            Assert.IsTrue(string.IsNullOrEmpty(activity.Speak));
            Assert.AreEqual(1, activity.Attachments.Count);
            Assert.AreEqual(HeroCard.ContentType, activity.Attachments[0].ContentType);
            var card = ((JObject)activity.Attachments[0].Content).ToObject<HeroCard>();
            Assert.IsNotNull(card, "should have herocard");
            Assert.AreEqual("Cheese gromit!", card.Title, "card title should be set");
            Assert.AreEqual("herocard", card.Subtitle, "card subtitle should be data bound ");
            Assert.AreEqual("This is some text describing the card, it's cool because it's cool", card.Text, "card text should be set");
            Assert.AreEqual("https://memegenerator.net/img/instances/500x/73055378/cheese-gromit.jpg", card.Images[0].Url, "image should be set");
            Assert.AreEqual("https://memegenerator.net/img/instances/500x/73055378/cheese-gromit.jpg", card.Images[1].Url, "image should be set");
            Assert.AreEqual(3, card.Buttons.Count, "card buttons should be set");
            for (int i = 0; i <= 2; i++)
            {
                Assert.AreEqual($"Option {i + 1}", card.Buttons[i].Title, "card buttons should be set");
            }

            // TODO add all of the other property types	
        }

        [TestMethod]
        public async Task TestThmbnailCard()
        {
            var context = await GetTurnContext("NonAdaptiveCardActivity.lg");
            var mg = new MessageActivityGenerator();
            dynamic data = new JObject();
            data.type = "thumbnailcard";
            IMessageActivity activity = await mg.Generate(context, "[ThumbnailCardTemplate]", data: data);
            Assert.AreEqual(ActivityTypes.Message, activity.Type);
            Assert.IsTrue(string.IsNullOrEmpty(activity.Text));
            Assert.IsTrue(string.IsNullOrEmpty(activity.Speak));
            Assert.AreEqual(1, activity.Attachments.Count);
            Assert.AreEqual(ThumbnailCard.ContentType, activity.Attachments[0].ContentType);
            var card = ((JObject)activity.Attachments[0].Content).ToObject<ThumbnailCard>();
            Assert.IsNotNull(card, "should have herocard");
            Assert.AreEqual("Cheese gromit!", card.Title, "card title should be set");
            Assert.AreEqual("thumbnailcard", card.Subtitle, "card subtitle should be data bound ");
            Assert.AreEqual("This is some text describing the card, it's cool because it's cool", card.Text, "card text should be set");
            Assert.AreEqual("https://memegenerator.net/img/instances/500x/73055378/cheese-gromit.jpg", card.Images[0].Url, "image should be set");
            Assert.AreEqual("https://memegenerator.net/img/instances/500x/73055378/cheese-gromit.jpg", card.Images[1].Url, "image should be set");
            Assert.AreEqual(3, card.Buttons.Count, "card buttons should be set");
            for (int i = 0; i <= 2; i++)
            {
                Assert.AreEqual($"Option {i + 1}", card.Buttons[i].Title, "card buttons should be set");
            }
        }

        //[TestMethod]
        //public async Task TestAdaptiveCard()
        //{
        //    var context = await GetTurnContext("AdaptiveCardActivity.lg");
        //    var mg = new MessageActivityGenerator();
        //    dynamic data = new JObject();
        //    data.adaptiveCardTitle = "test";
        //    IMessageActivity activity = await mg.Generate(context, "[adaptiveCardTemplate]", data: data);
        //    Assert.AreEqual(ActivityTypes.Message, activity.Type);
        //    Assert.IsTrue(string.IsNullOrEmpty(activity.Text));
        //    Assert.IsTrue(string.IsNullOrEmpty(activity.Speak));
        //    Assert.AreEqual(1, activity.Attachments.Count);
        //    Assert.AreEqual("application/vnd.microsoft.card.adaptive", activity.Attachments[0].ContentType);
        //    Assert.AreEqual("test", (string)((dynamic)activity.Attachments[0].Content).body[0].text);
        //}

        private static string GetProjectFolder()
        {
            return AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin"));
        }

        private ITurnContext GetTurnContext(ILanguageGenerator lg)
        {
            var context = new TurnContext(new TestAdapter(), new Activity());
            context.TurnState.Add<ILanguageGenerator>(lg);
            return context;
        }

        private async Task<ITurnContext> GetTurnContext(string lgFile)
        {
            var context = new TurnContext(new TestAdapter(), new Activity());
            var lgText = await resourceExplorer.GetResource(lgFile).ReadTextAsync();
            context.TurnState.Add<ILanguageGenerator>(new TemplateEngineLanguageGenerator(lgText, "test", LanguageGeneratorManager.ResourceResolver(resourceExplorer)));
            return context;
        }
    }

    public class MockLanguageGenerator : ILanguageGenerator
    {
        public Task<object> Generate(ITurnContext turnContext, string template, object data)
        {
            object result = template;
            return Task.FromResult(result);
        }
    }
}