using AiChef.Shared;
using AiChef.Server.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AiChef.Server.Services;

namespace AiChef.Server.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class RecipeController : ControllerBase
    {
        private readonly IOpenAIAPI _openAIservice;

        public RecipeController(IOpenAIAPI openAIservice)
        {
            _openAIservice = openAIservice;
        }


        //post to send the information
        [HttpPost, Route("GetRecipeIdeas")]
        public async Task<ActionResult<List<Idea>>> GetRecipeIdeas(RecipeParms recipeParms)
        {
            string mealtime = recipeParms.MealTime;
            List<string> ingredients = recipeParms.Ingredients
                                                   .Where(x => !string.IsNullOrEmpty(x.Description))
                                                   .Select(x => x.Description!)
                                                   .ToList();
            //null check
            if (string.IsNullOrEmpty(mealtime))
            {
                mealtime = "Breakfast";
            }       
            
            //service call
            var ideas = await _openAIservice.CreateRecipeIdeas(mealtime, ingredients);
            return ideas;

            //this return is for testing purposes
            //return SampleData.RecipeIdeas;
        }


        [HttpPost, Route("GetRecipe")]
        public async Task<ActionResult<Recipe?>> GetRecipe(RecipeParms recipeParms)
        {
            return SampleData.Recipe;
        }

        [HttpGet, Route("GetRecipeImage")]
        public async Task<RecipeImage> GetRecipeImage(string title)
        {
            return SampleData.RecipeImage;
        }


    }
}
