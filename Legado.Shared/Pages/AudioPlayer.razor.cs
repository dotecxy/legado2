using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Legado.Shared.Pages
{
    public partial class AudioPlayer
    {

        private void UpdateProgress(object sender, EventArgs e)
        {
            InvokeAsync(() =>
            {
                _currentTime = AudioService.CurrentPositionMillisecond / 1000;
                totalTime = AudioService.CurrentDurationMillisecond / 1000;
            });
            InvokeAsync(StateHasChanged); 
        }

        private async Task TogglePlayAsync()
        {
            if (AudioService.IsPlaying)
            {
                await AudioService.PauseAsync();
            }
            else
            {
                await AudioService.PlayAsync(AudioService.CurrentPositionMillisecond);
            }
        }


    }
}
