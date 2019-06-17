using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using System.Data;
using System.Data.SqlClient;
using System.Net.Http;
using GreatWall.Helpers;

namespace GreatWall.Dialogs
{
    [Serializable]
    public class OrderDialog : IDialog<string>
    {
        string strMessage;
        string strServerUrl = "http://localhost:3984/Images/";

        string strDBServer = "Server=tcp:medicine-db-server.database.windows.net,1433;"
                             + "Initial Catalog = MedicineDB; Persist Security Info=False;"
                             + "User ID=jmg970;Password=gerrad147@;"
                             + "MultipleActiveResultSets=False;Encrypt=True;"
                             + "TrustServerCertificate=False;"
                             + "Connection Timeout = 30;";

        string strSQL; //쿼리정보를 담는곳
        string strFeature; //선택한 분류 정보를 담는곳
        string strMedicineNm, strEchelon, strShape, strFrontSign, strBehindSign, strFrontColor, strBehindColor, strFrontLine, strBehindLine; //쿼리 where절에 들어갈 조건 필드들

        private string strSelected;

        public OrderDialog(string strSelected)
        {
            this.strSelected = strSelected;
        }

        public async Task StartAsync(IDialogContext context)
        {
            strMessage = null;

            await this.MessageReceivedAsync(context, null);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            if (result != null)
            {
                Activity activity = await result as Activity;

                string strRecvMsg = activity.Text.Trim();

                if(strRecvMsg.Substring(0, 2).Equals("내가"))
                {
                    strRecvMsg = "Other Medicine Search"; //약을 선택하여 종료되었을 때 새로 의약품 찾는 로직이 실행되도록 메시지 교체
                }

                if (strRecvMsg.Equals("Other Medicine Search"))
                {
                    await context.PostAsync("새로 찾기!");
                    strSQL = null; //쿼리 내용 초기화
                    context.Done("Medicine Bot Reload");
                }
                else
                {

                    if (strFeature.Equals("제형"))
                    {
                        strEchelon = strRecvMsg;
                    }
                    else if(strFeature.Equals("모양"))
                    {
                        strShape = strRecvMsg;
                    }
                    else if (strFeature.Equals("표시(앞)"))
                    {
                        strFrontSign = strRecvMsg;
                    }
                    else if (strFeature.Equals("표시(뒤)"))
                    {
                        strBehindSign = strRecvMsg;
                    }
                    else if (strFeature.Equals("색깔(앞)"))
                    {
                        strFrontColor = strRecvMsg;
                    }
                    else if (strFeature.Equals("색깔(뒤)"))
                    {
                        strBehindColor = strRecvMsg;
                    }
                    else if (strFeature.Equals("분할선(앞)"))
                    {
                        strFrontLine = strRecvMsg;
                    }
                    else if (strFeature.Equals("분할선(뒤)"))
                    {
                        strBehindLine = strRecvMsg;
                    }
                    /*
                    System.Diagnostics.Debug.WriteLine("==================================");
                    System.Diagnostics.Debug.WriteLine(strRecvMsg);
                    System.Diagnostics.Debug.WriteLine(strRecvMsg.Substring(0,2));
                    System.Diagnostics.Debug.WriteLine(strFeature);
                    System.Diagnostics.Debug.WriteLine("==================================");
                    */
                    if (strFeature.Equals("특징 선택 끝"))
                    {
                        strSQL = null; //쿼리 내용 초기화
                        context.Done("Medicine Bot Reload");
                    }
                    else
                    {
                        var message = context.MakeMessage();
                        var actions = new List<CardAction>();

                        actions.Add(new CardAction() { Title = "1. 제형", Value = "제형", Type = ActionTypes.ImBack });
                        actions.Add(new CardAction() { Title = "2. 모양", Value = "모양", Type = ActionTypes.ImBack });
                        actions.Add(new CardAction() { Title = "3. 표시(앞)", Value = "표시(앞)", Type = ActionTypes.ImBack });
                        actions.Add(new CardAction() { Title = "4. 표시(뒤)", Value = "표시(뒤)", Type = ActionTypes.ImBack });
                        actions.Add(new CardAction() { Title = "5. 색깔(앞)", Value = "색깔(앞)", Type = ActionTypes.ImBack });
                        actions.Add(new CardAction() { Title = "6. 색깔(뒤)", Value = "색깔(뒤)", Type = ActionTypes.ImBack });
                        actions.Add(new CardAction() { Title = "7. 분할선(앞)", Value = "분할선(앞)", Type = ActionTypes.ImBack });
                        actions.Add(new CardAction() { Title = "8. 분할선(뒤)", Value = "분할선(뒤)", Type = ActionTypes.ImBack });
                        actions.Add(new CardAction() { Title = "9. 특징 선택 끝", Value = "특징 선택 끝", Type = ActionTypes.ImBack });

                        message.Attachments.Add(new HeroCard { Title = "의약품의 어떤 특징으로 찾을지 선택해주세요.", Buttons = actions }.ToAttachment());

                        await context.PostAsync(message);

                        context.Wait(selectFeatureAsync);
                    }

                }

            }
            else
            {

                if(strSelected.Equals("명칭"))
                {
                    await context.PostAsync("의약품 명을 입력해주세요.");
                    context.Wait(resultMessageAsync);
                }
                else if (strSelected.Equals("특징"))
                {
                    var message = context.MakeMessage();
                    var actions = new List<CardAction>();

                    actions.Add(new CardAction() { Title = "1. 제형", Value = "제형", Type = ActionTypes.ImBack });
                    actions.Add(new CardAction() { Title = "2. 모양", Value = "모양", Type = ActionTypes.ImBack });
                    actions.Add(new CardAction() { Title = "3. 표시(앞)", Value = "표시(앞)", Type = ActionTypes.ImBack });
                    actions.Add(new CardAction() { Title = "4. 표시(뒤)", Value = "표시(뒤)", Type = ActionTypes.ImBack });
                    actions.Add(new CardAction() { Title = "5. 색깔(앞)", Value = "색깔(앞)", Type = ActionTypes.ImBack });
                    actions.Add(new CardAction() { Title = "6. 색깔(뒤)", Value = "색깔(뒤)", Type = ActionTypes.ImBack });
                    actions.Add(new CardAction() { Title = "7. 분할선(앞)", Value = "분할선(앞)", Type = ActionTypes.ImBack });
                    actions.Add(new CardAction() { Title = "8. 분할선(뒤)", Value = "분할선(뒤)", Type = ActionTypes.ImBack });
                    actions.Add(new CardAction() { Title = "9. 특징 선택 끝", Value = "특징 선택 끝", Type = ActionTypes.ImBack });

                    message.Attachments.Add(new HeroCard { Title = "의약품의 어떤 특징으로 찾을지 선택해주세요.", Buttons = actions }.ToAttachment());

                    await context.PostAsync(message);

                    context.Wait(selectFeatureAsync);
                }

            }

        }

        public async Task selectFeatureAsync(IDialogContext context, IAwaitable<object> result)
        {
            Activity activity = await result as Activity;
            strFeature = activity.Text.Trim();

            if (strFeature.Equals("제형"))
            {
                var message = context.MakeMessage();
                var actions = new List<CardAction>();

                actions.Add(new CardAction() { Title = "1. 필름코팅정", Value = "필름코팅정", Type = ActionTypes.ImBack });
                actions.Add(new CardAction() { Title = "2. 연질캡슐", Value = "연질캡슐", Type = ActionTypes.ImBack });
                actions.Add(new CardAction() { Title = "3. 경질캡슐", Value = "경질캡슐", Type = ActionTypes.ImBack });
                actions.Add(new CardAction() { Title = "4. 트로키정", Value = "트로키정", Type = ActionTypes.ImBack });
                actions.Add(new CardAction() { Title = "5. 저작정", Value = "저작정", Type = ActionTypes.ImBack });
                actions.Add(new CardAction() { Title = "6. 당의정", Value = "당의정", Type = ActionTypes.ImBack });
                actions.Add(new CardAction() { Title = "7. 나정", Value = "나정", Type = ActionTypes.ImBack });

                message.Attachments.Add(new HeroCard { Title = "의약품의 제형을 선택해주세요.", Buttons = actions }.ToAttachment());

                await context.PostAsync(message);

                context.Wait(MessageReceivedAsync);
            }
            else if (strFeature.Equals("모양"))
            {
                var message = context.MakeMessage();
                var actions = new List<CardAction>();

                actions.Add(new CardAction() { Title = "1. 타원형", Value = "타원형", Type = ActionTypes.ImBack });
                actions.Add(new CardAction() { Title = "2. 장방형", Value = "장방형", Type = ActionTypes.ImBack });
                actions.Add(new CardAction() { Title = "3. 오각형", Value = "오각형", Type = ActionTypes.ImBack });
                actions.Add(new CardAction() { Title = "4. 팔각형", Value = "팔각형", Type = ActionTypes.ImBack });
                actions.Add(new CardAction() { Title = "5. 도넛형", Value = "도넛형", Type = ActionTypes.ImBack });
                actions.Add(new CardAction() { Title = "6. 8자형", Value = "8자형", Type = ActionTypes.ImBack });
                actions.Add(new CardAction() { Title = "7. 원형", Value = "원형", Type = ActionTypes.ImBack });

                message.Attachments.Add(new HeroCard { Title = "의약품의 모양을 선택해주세요.", Buttons = actions }.ToAttachment());

                await context.PostAsync(message);

                context.Wait(MessageReceivedAsync);
            }
            else if (strFeature.Equals("표시(앞)"))
            {
                await context.PostAsync("의약품 앞면에 표시되어 있는 문자를 입력해주세요.");
                context.Wait(MessageReceivedAsync);
            }
            else if (strFeature.Equals("표시(뒤)"))
            {
                await context.PostAsync("의약품 뒷면에 표시되어 있는 문자를 입력해주세요.");
                context.Wait(MessageReceivedAsync);
            }
            else if (strFeature.Equals("색깔(앞)"))
            {
                await context.PostAsync("의약품 앞면의 색상을 입력해주세요.\nex)하양,노랑,분홍,갈색,연두,초록,빨강,청록...");
                context.Wait(MessageReceivedAsync);
            }
            else if (strFeature.Equals("색깔(뒤)"))
            {
                await context.PostAsync("의약품 뒷면의 색상을 입력해주세요.\nex)하양,노랑,분홍,갈색,연두,초록,빨강,청록...");
                context.Wait(MessageReceivedAsync);
            }
            else if (strFeature.Equals("분할선(앞)"))
            {
                await context.PostAsync("의약품 앞면의 분할선을 입력해주세요.\nex)'+','-'");
                context.Wait(MessageReceivedAsync);
            }
            else if (strFeature.Equals("분할선(뒤)"))
            {
                await context.PostAsync("의약품 뒷면의 분할선을 입력해주세요.\nex)'+','-'");
                context.Wait(MessageReceivedAsync);
            }
            else if(strFeature.Equals("특징 선택 끝"))
            {
                await context.PostAsync("현재 선택하신 의약품의 특징\n"
                                        + (string.IsNullOrEmpty(strEchelon) ? "" : "제형 ☞ " + strEchelon + "\n")
                                        + (string.IsNullOrEmpty(strShape) ? "" : "모양 ☞ " + strShape + "\n")
                                        + (string.IsNullOrEmpty(strFrontSign) ? "" : "표시(앞) ☞ " + strFrontSign + "\n")
                                        + (string.IsNullOrEmpty(strBehindSign) ? "" : "표시(뒤) ☞ " + strBehindSign + "\n")
                                        + (string.IsNullOrEmpty(strFrontColor) ? "" : "색깔(앞) ☞ " + strFrontColor + "\n")
                                        + (string.IsNullOrEmpty(strBehindColor) ? "" : "색깔(뒤) ☞ " + strBehindColor + "\n")
                                        + (string.IsNullOrEmpty(strFrontLine) ? "" : "분할선(앞) ☞ " + strFrontLine + "\n")
                                        + (string.IsNullOrEmpty(strBehindLine) ? "" : "분할선(뒤) ☞ " + strBehindLine + "\n")
                                        + "위 내용대로 선택하신게 맞습니까?? (Yes or No)");
                context.Wait(resultMessageAsync);
            }
        }

        public async Task resultMessageAsync(IDialogContext context, IAwaitable<object> result)
        {
            Activity activity = await result as Activity;
            string strResult = activity.Text.Trim();

            if (strSelected.Equals("명칭"))
            {
                strMedicineNm = strResult;
                strSQL = "SELECT * FROM Medicine WHERE MEDICINE_NM = N'" + strMedicineNm + "'";
            }
            else
            {
                if(strResult.ToLower().Equals("yes"))
                {
                    strSQL = "SELECT * FROM Medicine " +
                             "WHERE ECHELON LIKE N'%" + strEchelon + "%' AND SHAPE LIKE N'%" + strShape + "%'" +
                             "AND FRONT_SIGN LIKE N'%" + strFrontSign + "%' AND BEHIND_SIGN LIKE N'%" + strBehindSign + "%'" +
                             "AND FRONT_COLOR LIKE N'%" + strFrontColor + "%' AND BEHIND_COLOR LIKE N'%" + strBehindColor + "%'" +
                             "AND FRONT_LINE LIKE N'%" + strFrontLine + "%' AND BEHIND_LINE LIKE N'%" + strBehindLine + "%'";
                }
                else{
                    await context.PostAsync("No를 입력하셨으므로 다시 초기화면으로 돌아갑니다.");
                    context.Done("Medicine Bot Reload");
                }
            }

            if (!strResult.ToLower().Equals("no"))
            {
                strMessage = "[Medicine] 입력한 조건으로 조회 된 의약품 리스트 입니다.";
                await context.PostAsync(strMessage);

                //DB Connection
                SqlConnection DB_CON = new SqlConnection(strDBServer);
                SqlCommand DB_Query = new SqlCommand(strSQL, DB_CON);
                SqlDataAdapter DB_Adapter = new SqlDataAdapter(DB_Query);

                DataSet DB_DS = new DataSet();
                DB_Adapter.Fill(DB_DS);

                //Medicine
                var message = context.MakeMessage();
                foreach (DataRow row in DB_DS.Tables[0].Rows)
                {
                    message.Attachments.Add(CardHelper.GetHeroCard(row["MEDICINE_NM"].ToString(),
                                            row["EFFECT"].ToString(), this.strServerUrl + row["MEDICINE_NO"].ToString() + ".jpg",
                                            "선택", "내가 찾던 약의 명칭은 "+row["MEDICINE_NM"].ToString()+"이고\n효능은 "+ row["EFFECT"].ToString()+"이구나!"));
                }

                message.Attachments.Add(CardHelper.GetHeroCard("다른 의약품 찾기", "Other Medicine Search", null, "다른 의약품 찾기", "Other Medicine Search"));
                message.AttachmentLayout = "carousel";
                await context.PostAsync(message);
                context.Wait(this.MessageReceivedAsync);
            }
            
        }

    }
}