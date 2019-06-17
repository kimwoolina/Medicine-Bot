using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GreatWall.Helpers
{
    public static class CardHelper
    {
        public static Attachment GetHeroCard(string strTitle, string strSubTitle, string strImage, string strButtonText, string strButtonValue)
        {
            List<CardImage> images = new List<CardImage>();
            images.Add(new CardImage() { Url = strImage });

            List<CardAction> buttons = new List<CardAction>();
            buttons.Add(new CardAction() { Title = strButtonText, Value = strButtonValue, Type = ActionTypes.ImBack });

            HeroCard card = new HeroCard()
            {
                Title = strTitle,
                Subtitle = strSubTitle,
                Images = images,
                Buttons = buttons
            };

            return card.ToAttachment();
        }

    }
}