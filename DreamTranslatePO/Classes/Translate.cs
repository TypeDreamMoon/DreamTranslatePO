namespace DreamTranslatePO.Classes
{
    namespace Translate
    {
        public class TranslateEngineSetting
        {
            public string? Icon;
            public string TranslateEngineName;
            public string ApiUrl;
            public string ApiKey;
            public string ServerUrl;
            public bool RequiresAnApi;
            public bool ServerMode;

            public TranslateEngineSetting()
            {
                ApiUrl = "null";
                ApiKey = "null";
                ServerUrl = "null";
                RequiresAnApi = false;
                ServerMode = false;
                TranslateEngineName = "null";
            }
        }

        /// <summary>
        /// Translate Engine Impl
        /// </summary>
        public class TranslateEngine
        {
        }

        public class TranslateEngineAi : TranslateEngine
        {
        }

        public class TranslateEngineMicrosoft : TranslateEngine
        {
        }
        
        public class TranslateEngineGoogle : TranslateEngine
        {
        }
        
        public class TranslateEngineBaidu : TranslateEngine
        {
        }
        
        
        public class TranslateEngineDeepL : TranslateEngine
        {
        }
        
        public class TranslateEngineYouDao : TranslateEngine
        {
        }
        
        public class TranslateEngineTencent : TranslateEngine
        {
        }
    }
}