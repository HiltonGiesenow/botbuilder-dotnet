using System;
using Microsoft.Bot.Builder.Dialogs.Declarative.Loaders;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Microsoft.Bot.Builder.LanguageGeneration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs
{
    public static class LGAdapterExtensions
    {
        /// <summary>
        /// Register default LG file as language generation.
        /// </summary>
        /// <param name="botAdapter">The <see cref="BotAdapter"/> to add services to.</param>
        /// <param name="resourceExplorer">resource explorer.</param>
        /// <param name="defaultLg">Default LG Resource Id (default: main.lg).</param>
        /// <param name="avtivityGenerator">Optional message generator.</param>
        /// <returns>The BotAdapter.</returns>
        public static BotAdapter UseLanguageGeneration(
            this BotAdapter botAdapter, 
            ResourceExplorer resourceExplorer, 
            string defaultLg = null,
            IActivityGenerator avtivityGenerator = null)
        {
            if (defaultLg == null)
            {
                defaultLg = "main.lg";
            }

            // if there is no main.lg, then provide default engine (for inline expression evaluation only)
            if (resourceExplorer.GetResource(defaultLg) == null)
            {
                botAdapter.UseLanguageGeneration(resourceExplorer, new TemplateEngineLanguageGenerator(string.Empty, defaultLg, LanguageGeneratorManager.ResourceResolver(resourceExplorer)));
            }
            else
            {
                botAdapter.UseLanguageGeneration(resourceExplorer, new ResourceMultiLanguageGenerator(defaultLg));
            }

            return botAdapter;
        }

        /// <summary>
        /// Register ILanguageGenerator as default langugage generator.
        /// </summary>
        /// <param name="botAdapter">botAdapter to add services to.</param>
        /// <param name="resourceExplorer">resourceExporer to provide to LanguageGenerator.</param>
        /// <param name="languageGenerator">LanguageGenerator to use.</param>
        /// <param name="activityGenerator">(OPTIONAL) Default is ActivityGenerator(). </param>
        /// <returns>botAdapter.</returns>
        public static BotAdapter UseLanguageGeneration(this BotAdapter botAdapter, ResourceExplorer resourceExplorer, ILanguageGenerator languageGenerator, IActivityGenerator activityGenerator = null)
        {
            TypeFactory.Register("DefaultLanguageGenerator", typeof(ResourceMultiLanguageGenerator), new CustomLGLoader());
            botAdapter.Use(new RegisterClassMiddleware<LanguageGeneratorManager>(new LanguageGeneratorManager(resourceExplorer ?? throw new ArgumentNullException(nameof(resourceExplorer)))));
            botAdapter.Use(new RegisterClassMiddleware<ILanguageGenerator>(languageGenerator ?? throw new ArgumentNullException(nameof(languageGenerator))));
            botAdapter.UseActivityGeneration(activityGenerator);
            return botAdapter;
        }

        /// <summary>
        /// Register MessageActivityGeneration. 
        /// </summary>
        /// <param name="botAdapter">botAdapter to add services to.</param>
        /// <param name="activityGenerator">(OPTIONAL) Default is ActivityGenerator(). </param>
        /// <returns>botAdapter.</returns>
        public static BotAdapter UseActivityGeneration(this BotAdapter botAdapter, IActivityGenerator activityGenerator = null)
        {
            botAdapter.Use(new RegisterClassMiddleware<IActivityGenerator>(activityGenerator ?? new ActivityGenerator()));
            return botAdapter;
        }

        internal class CustomLGLoader : ICustomDeserializer
        {
            public object Load(JToken obj, JsonSerializer serializer, Type type)
            {
                return new ResourceMultiLanguageGenerator(obj.Value<string>());
            }
        }
    }
}
