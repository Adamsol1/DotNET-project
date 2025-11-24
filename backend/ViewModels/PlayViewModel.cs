// ViewModel for the game play page.
// It passes the player's character ID and the current story node (scene)
// from the controller to the Razor view, so the view can render
// the correct story text, background, and available choices.
using backend.Application.Dtos;

namespace backend.ViewModels
{
    public class PlayViewModel
    {
        public int CurrentNodeId { get; set; }
        public StoryNodeDto Node { get; set; } = new StoryNodeDto();
    }
}
