using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;

namespace GreatWall.Dialogs
{
    [Serializable]
    public class FAQDialog : IDialog<string>
    {
        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("FAQ Service: ");
            context.Wait(MessageReceivedAsync);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            Activity activity = await result as Activity;

            if (activity.Text.Trim() == "Exit")
            {
                context.Done("Order Completed");
            }
            else
            {
                await context.PostAsync("FAQ Dialog.");
                context.Wait(MessageReceivedAsync);
            }
        }
    }
}