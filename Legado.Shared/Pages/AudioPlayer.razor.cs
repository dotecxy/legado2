using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Legado.Shared.Pages
{
    public partial class AudioPlayer
    {
         

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
