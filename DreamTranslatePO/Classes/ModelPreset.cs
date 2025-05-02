namespace DreamTranslatePO.Classes
{
    public class ModelPreset
    {
        public string DisplayName
        {
            get;
            set;
        }

        public string BaseUrl
        {
            get;
            set;
        }

        public List<string> Models
        {
            get;
            set;
        }

        public ModelPreset()
        {
            DisplayName = "null";
            BaseUrl = "null";
            Models = new List<string>() { "null" };
        }

        public static ModelPreset SiliconFlow()
        {
            return new ModelPreset()
            {
                DisplayName = "SiliconFlow",
                BaseUrl = "https://api.siliconflow.cn/v1/chat/completions",
                Models = new List<string>()
                {
                    "Pro/deepseek-ai/DeepSeek-V3-1226",
                    "Pro/deepseek-ai/DeepSeek-V3",
                    "Pro/deepseek-ai/DeepSeek-R1",
                    "deepseek-ai/DeepSeek-V3",
                    "deepseek-ai/DeepSeek-R1",
                    "Qwen/Qwen2.5-VL-32B-Instruct",
                    "Qwen/Qwen3-235B-A22B",
                    "Qwen/Qwen3-30B-A3B",
                    "Qwen/Qwen3-32B",
                    "Qwen/Qwen3-14B",
                    "Qwen/Qwen3-8B",
                    "Qwen/QwQ-32B",
                    "THUDM/GLM-Z1-9B-0414",
                    "THUDM/GLM-4-9B-0414",
                }
            };
        }

        public static ModelPreset Deepseek()
        {
            return new ModelPreset()
            {
                DisplayName = "Deepseek",
                BaseUrl = "https://api.deepseek.com/chat/completions",
                Models = new List<string>()
                {
                    "deepseek-chat",
                    "deepseek-reasoner",
                }
            };
        }

        public static ModelPreset OpenAI()
        {
            return new ModelPreset()
            {
                DisplayName = "OpenAI",
                BaseUrl = "https://api.openai.com/v1/chat/completions",
                Models = new List<string>()
                {
                    "o4-mini-2025-04-16",
                    "o3-2025-04-16",
                    "gpt-4.1-2025-04-14",
                    "o1-mini-2024-09-12",
                }
            };
        }

        public static ModelPreset Kimi()
        {
            return new ModelPreset()
            {
                DisplayName = "Kimi",
                BaseUrl = "https://api.moonshot.cn/v1/chat/completions",
                Models = new List<string>()
                {
                    "moonshot-v1-8k",
                    "moonshot-v1-32k",
                    "moonshot-v1-128k",
                }
            };
        }

        public static ModelPreset Hunyuan()
        {
            return new ModelPreset()
            {
                DisplayName = "Hunyuan",
                BaseUrl = "https://api.hunyuan.cloud.tencent.com/v1",
                Models = new List<string>()
                {
                    "hunyuan-turbos-latesthunyuan-turbos-latest",
                }
            };
        }

        public static ModelPreset Custom()
        {
            return new ModelPreset()
            {
                DisplayName = "Custom",
                BaseUrl = "null",
                Models = new List<string>()
                {
                    "null",
                }
            };
        }

        public bool IsEqual(ModelPreset test)
        {
            return this.DisplayName == test.DisplayName;
        }
    }
}