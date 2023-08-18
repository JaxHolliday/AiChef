using AiChef.Shared;
using AiChef.Server.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AiChef.Server.Services;
using System.Reflection.Metadata.Ecma335;

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
            //return SampleData.Recipe;

            List<string> ingredients = recipeParms.Ingredients
                                                  .Where(x => !string.IsNullOrEmpty(x.Description))
                                                  .Select(x => x.Description!)
                                                  .ToList();

            string? title = recipeParms.SelectedIdea;

            if (string.IsNullOrEmpty(title))
            {
                //standard http return request
                return BadRequest();
            }

            var recipe = await _openAIservice.CreateRecipe(title, ingredients);
            return recipe;

        }

        [HttpGet, Route("GetRecipeImage")]
        public async Task<RecipeImage> GetRecipeImage(string title)
        {
            //return SampleData.RecipeImage;

            var recipeImage = await _openAIservice.CreateRecipeImage(title);

            return recipeImage;
        }


    }
}
