using AiChef.Shared;
using AIChef.Server.ChatEndPoint;
using System.Net.Http.Headers;
using System.Text.Json;

namespace AiChef.Server.Services
{
    public class OpenAIService : IOpenAIAPI
    {
        private readonly IConfiguration _configuration;
        private static readonly string _baseUrl = "https://api.openai.com/v1/";
        private static readonly HttpClient _httpClient = new();
        private readonly JsonSerializerOptions _jsonOptions;

        // build the function object so the AI will return JSON formatted object
        private static ChatFunction.Parameter _recipeIdeaParameter = new()
        {
            // describes one Idea
            Type = "object",
            Required = new string[] { "index", "title", "description" },
            Properties = new
            {
                // provide a type and description for each property of the Idea model
                Index = new ChatFunction.Property
                {
                    Type = "number",
                    Description = "A unique identifier for this object",
                },
                Title = new ChatFunction.Property
                {
                    Type = "string",
                    Description = "The name for a recipe to create"
                },
                Description = new ChatFunction.Property
                {
                    Type = "string",
                    Description = "A description of the recipe"
                }
            }
        };

        private static ChatFunction _ideaFunction = new()
        {
            // describe the function we want an argument for from the AI
            Name = "CreateRecipe",
            // this description ensures we get 5 ideas back
            Description = "Generates recipes for each idea in an array of five recipe ideas",
            Parameters = new
            {
                // OpenAI requires that the parameters are an object, so we need to wrap our array in an object
                Type = "object",
                Properties = new
                {
                    Data = new // our array will come back in an object in the Data property
                    {
                        Type = "array",
                        // further ensures the AI will create 5 recipe ideas
                        Description = "An array of five recipe ideas",
                        Items = _recipeIdeaParameter
                    }
                }
            }
        };
        
        //standard based on whatever API youi are using 
        public OpenAIService(IConfiguration configuration)
        {
            _configuration = configuration;
            //grab api key out of host or railway
            var apiKey = _configuration["OpenAi:OpenAiKey"] ?? Environment.GetEnvironmentVariable("OpenAiKey");

            //string will be put into request header
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new("Bearer", apiKey);

            _jsonOptions = new()
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };

        }

        public async Task<List<Idea>> CreateRecipeIdeas(string mealtime, List<string> ingredientList)
        {
            //private variable setup
            string url = $"{_baseUrl}chat/completions";
            string systemPrompt = "You are a world-renowned chef. I will send you a list of ingredients and a meal time. You will respond with five ideas for dishes.";
            string userPrompt = "";
            string ingredientPrompt = "";

            //creating out ingreidnet list into a list of strings
            string ingredients = string.Join(",", ingredientList);

            //checking is list is null or empty
            if (string.IsNullOrEmpty(ingredients))
            {
                ingredientPrompt = "Suggest some ingredients for me";
            }
            else
            {
                ingredientPrompt = $"I Have {ingredients}";
            }

            //meal followed by the ingredient list 
            userPrompt = $"The meal I want to cook is {mealtime}. {ingredientPrompt}";
            ChatMessage systemMessage = new()
            {
                Role = "system", 
                Content = $"{systemPrompt}"
            };

            ChatMessage userMessage = new()
            {
                Role = "user",
                Content = $"{userPrompt}"
            };

            ChatRequest request = new()
            {
                //use model we want exspressly 
                Model = "gpt-3.5-turbo-0613",
                Messages = new[] { systemMessage, userMessage },
                Functions = new[] { _ideaFunction },
                FunctionCall = new ChatFunctionResponse { Name = _ideaFunction.Name }
            };

            //call to OpenAi with the request object and how to format
            HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync(url, request, _jsonOptions);
            // looking at the content for the response 
            ChatResponse? response = await httpResponse.Content.ReadFromJsonAsync<ChatResponse>();

            //get the first message in the function call/ obj that is in "response"
            ChatFunctionResponse? functionResponse = response.Choices?
                                                             .FirstOrDefault(m => m.Message?.FunctionCall is not null)?
                                                             .Message?
                                                             .FunctionCall;
        }
    }
}
