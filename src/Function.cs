using System;
using System.Collections.Generic;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace JasonCable.TodaysMoney
{
    public class Function
    {
        public SkillResponse FunctionHandler(SkillRequest input, ILambdaContext context)
        {
            try
            {
                if (input == null || input.Request == null)
                {
                    return ResponseBuilder.Tell("i didn't understand that.  goodbye.");
                }

                Type requestType = input.Request.GetType();

                if (requestType == typeof(CanFulfillIntentRequest))
                {
                    var request = input.Request as CanFulfillIntentRequest;

                    CanFulfillIntent response = new CanFulfillIntent();
                    response.Slots = new Dictionary<string, CanfulfillSlot>();

                    if (request.Intent.Name == "ComputeInflation")
                    {
                        bool hasYear, hasDollars, goodYear, goodDollars;
                        hasYear = hasDollars = goodYear = goodDollars = false;

                        if (request.Intent.Slots.ContainsKey("year"))
                        {
                            hasYear = true;

                            string slotValue = request.Intent.Slots["year"].Value;
                            int number;
                            if (!String.IsNullOrWhiteSpace(slotValue) && Int32.TryParse(slotValue, out number) &&
                                number >= 1774 && number <= 2018)
                            {
                                goodYear = true;
                            }
                        }

                        if (request.Intent.Slots.ContainsKey("dollars"))
                        {
                            hasDollars = true;

                            string slotValue = request.Intent.Slots["dollars"].Value;
                            int number;
                            if (!String.IsNullOrWhiteSpace(slotValue) && Int32.TryParse(slotValue, out number) &&
                                number > 0 && number < Int32.MaxValue)
                            {
                                goodDollars = true;
                            }
                        }

                        if (hasYear && goodYear && hasDollars && goodDollars)
                        {
                            response.CanFulfill = CanFulfill.YES;
                            response.Slots.Add("year", new CanfulfillSlot
                            {
                                CanFulfill = SlotCanFulfill.YES,
                                CanUnderstand = CanUnderstand.YES
                            });
                            response.Slots.Add("dollars", new CanfulfillSlot
                            {
                                CanFulfill = SlotCanFulfill.YES,
                                CanUnderstand = CanUnderstand.YES
                            });
                        }
                        else if (hasYear && goodYear && !hasDollars)
                        {
                            response.CanFulfill = CanFulfill.MAYBE;
                            response.Slots.Add("year", new CanfulfillSlot
                            {
                                CanFulfill = SlotCanFulfill.YES,
                                CanUnderstand = CanUnderstand.YES
                            });
                            response.Slots.Add("dollars", new CanfulfillSlot
                            {
                                CanFulfill = SlotCanFulfill.NO,
                                CanUnderstand = CanUnderstand.NO
                            });
                        }
                        else if (hasDollars && goodDollars && !hasYear)
                        {
                            response.CanFulfill = CanFulfill.MAYBE;
                            response.Slots.Add("year", new CanfulfillSlot
                            {
                                CanFulfill = SlotCanFulfill.NO,
                                CanUnderstand = CanUnderstand.NO
                            });
                            response.Slots.Add("dollars", new CanfulfillSlot
                            {
                                CanFulfill = SlotCanFulfill.YES,
                                CanUnderstand = CanUnderstand.YES
                            });
                        }
                        else
                        {
                            response.CanFulfill = CanFulfill.NO;
                            response.Slots.Add("year", new CanfulfillSlot
                            {
                                CanFulfill = SlotCanFulfill.NO,
                                CanUnderstand = CanUnderstand.NO
                            });
                            response.Slots.Add("dollars", new CanfulfillSlot
                            {
                                CanFulfill = SlotCanFulfill.NO,
                                CanUnderstand = CanUnderstand.NO
                            });
                        }
                    }
                    else
                    {
                        response.CanFulfill = CanFulfill.NO;
                    }

                    var sr = ResponseBuilder.Empty();
                    sr.Response.CanFulfillIntent = response;
                    return sr;
                }

                if (requestType == typeof(IntentRequest))
                {
                    var intentRequest = input.Request as IntentRequest;

                    if (intentRequest.Intent.Name == "AMAZON.HelpIntent")
                    {
                        var output = new PlainTextOutputSpeech();
                        output.Text =
                            "Inflation Calc can tell you how much money from a certain year is worth in today's dollars, for example, how much is one dollar in nineteen twelve worth today?  How many dollars do you wish to convert?";
                        return ResponseBuilder.Ask(output, new Reprompt("How many dollars?"));
                        //return ResponseBuilder.DialogElicitSlot(output, "dollars");
                    }
                    else if (intentRequest.Intent.Name == "ComputeInflation")
                    {
                        var year = intentRequest.Intent.Slots["year"].Value;
                        var dollars = intentRequest.Intent.Slots["dollars"].Value;

                        if (String.IsNullOrWhiteSpace(year) && String.IsNullOrWhiteSpace(dollars))
                        {
                            var output = new PlainTextOutputSpeech();
                            output.Text = "Inflation Calc wants to know how many dollars you want to convert.";
                            return ResponseBuilder.DialogElicitSlot(output, "dollars");
                        }

                        if (String.IsNullOrWhiteSpace(year))
                        {
                            var output = new PlainTextOutputSpeech();
                            output.Text = "from which year?";
                            return ResponseBuilder.DialogElicitSlot(output, "year");
                        }

                        if (String.IsNullOrWhiteSpace(dollars))
                        {
                            var output = new PlainTextOutputSpeech();
                            output.Text = "how many dollars?";
                            return ResponseBuilder.DialogElicitSlot(output, "dollars");
                        }

                        int yearInt, dollarsInt;

                        if (!Int32.TryParse(year, out yearInt))
                        {
                            var output = new PlainTextOutputSpeech();
                            output.Text = "i didn't understand what you said.  please ask for another year.";
                            return ResponseBuilder.DialogElicitSlot(output, "year");
                        }

                        if (!Int32.TryParse(dollars, out dollarsInt))
                        {
                            var output = new PlainTextOutputSpeech();
                            output.Text = "i can't understand what you said.  please ask for another dollar amount.";
                            return ResponseBuilder.DialogElicitSlot(output, "dollars");
                        }

                        if (yearInt < 1774 || yearInt > 2018)
                        {
                            var output = new PlainTextOutputSpeech();
                            output.Text =
                                "i can compute years between seventeen seventy four and two thousand eighteen.  please ask for another year.";
                            return ResponseBuilder.DialogElicitSlot(output, "year");

                            //return ResponseBuilder.DialogDelegate();
                        }

                        if (dollarsInt < 0 || dollarsInt > 5000000)
                        {
                            var output = new PlainTextOutputSpeech();
                            output.Text =
                                "i can convert dollar amounts between one and five million.  please ask for another dollar amount.";
                            return ResponseBuilder.DialogElicitSlot(output, "dollars");
                        }

                        double divisor = (double) typeof(ConversionFactor).GetField("Year" + year).GetValue(null);
                        double result = Math.Round(Convert.ToInt32(dollars) / divisor, 2);

                        string answer = "<speak>";
                        answer += SayNumber(dollars);
                        answer += " dollars in " + SayYear(year) + " is worth ";
                        answer += SayNumber(Convert.ToInt32(result).ToString());
                        var cents = Convert.ToInt32(Math.Round(result % 1 * 100, 0));
                        answer += " dollars and ";
                        answer += SayNumber(cents.ToString());
                        answer += " cents.";
                        answer += "</speak>";

                        var ssml = new SsmlOutputSpeech();
                        ssml.Ssml = answer;

                        var finalResponse = ResponseBuilder.Tell(ssml);
                        return finalResponse;
                    }
                }

                if (requestType == typeof(LaunchRequest))
                {
                    var output = new PlainTextOutputSpeech();
                    output.Text =
                        "Inflation Calc can tell you how much money from a certain year is worth in today's dollars, for example, how much is one dollar in nineteen twelve worth today?";
                    var reprompt = new PlainTextOutputSpeech();
                    reprompt.Text = "how many dollars?";
                    return ResponseBuilder.Ask(output, new Reprompt {OutputSpeech = reprompt});
                }
            }
            catch (Exception exc)
            {
                return ResponseBuilder.Tell("sorry, i was unable to understand that. goodbye.");
            }

            return ResponseBuilder.Tell("Ok.");
        }

        private string SayNumber(string s)
        {
            return $"<say-as interpret-as=\"cardinal\">{s}</say-as>";
        }

        private string SayYear(string s)
        {
            return $"<say-as interpret-as=\"date\" format=\"y\">{s}</say-as>";
        }
    }
}
