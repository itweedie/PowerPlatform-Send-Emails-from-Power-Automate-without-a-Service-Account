public class Script : ScriptBase
{
    public override async Task<HttpResponseMessage> ExecuteAsync()
    {
        // Read the request content as a string
        var requestContentAsString = await this.Context.Request.Content.ReadAsStringAsync().ConfigureAwait(false);
        
        // Parse the request content string into a JSON object
        var requestContentAsJson = JObject.Parse(requestContentAsString);

        // Modify the attachments array if it exists
        List<string> attachmentFileTypes = new List<string>();

        if (requestContentAsJson["message"]?["attachments"] is JArray attachments)
        {
            foreach (var attachment in attachments)
            {
                // Add the @odata.type element
                attachment["@odata.type"] = "#microsoft.graph.fileAttachment";
            }
        }
        
        // Set the modified JSON back to the request content
        this.Context.Request.Content = CreateJsonContent(requestContentAsJson.ToString());

        // Send the API request and get the response
        var response = await this.Context.SendAsync(this.Context.Request, this.CancellationToken).ConfigureAwait(continueOnCapturedContext: false);

        // Read the response content as a string
        var responseContentAsString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        // Check if the response content is empty or null
        if (string.IsNullOrEmpty(responseContentAsString))
        {
            // Set a default message if there is no response from the endpoint
            responseContentAsString = "{\"message\": \"No response from the endpoint\"}";
        }
        else
        {
            try
            {
                // Try to parse the response content string into a JSON object
                var responseContentAsJson = JObject.Parse(responseContentAsString);
                
                // Convert the JSON object back to a string
                responseContentAsString = responseContentAsJson.ToString();
            }
            catch (JsonReaderException)
            {
                // If parsing fails, set an error message with the invalid JSON response
                responseContentAsString = $"{{\"message\": \"Invalid JSON response\", \"response\": \"{responseContentAsString}\"}}";
            }
        }

        // Make a custom HTTP GET call to the developer messaging API
        string developerMessage = "Failed to get updated developer message";
        try
        {
            var request = (HttpWebRequest)WebRequest.Create("https://developer-message.mightora.io/api/HttpTrigger?appname=send-email-with-graph");
            request.Method = "GET";

            using (var developerResponse = (HttpWebResponse)request.GetResponse())
            {
                using (var streamReader = new StreamReader(developerResponse.GetResponseStream()))
                {
                    var developerResponseContent = streamReader.ReadToEnd();
                    var developerResponseJson = JObject.Parse(developerResponseContent);
                    developerMessage = developerResponseJson["message"]?.ToString() ?? developerMessage;
                }
            }
        }
        catch
        {
            // If the GET request fails, developerMessage remains as the default failure message
        }

        // Create a JSON object to include the original request, the response content, and the developer message
        var finalResponseContent = new JObject
        {
            ["version"] = "1.2.0", // Add version number here
            ["responseContent"] = JObject.Parse(responseContentAsString),
            ["developerMessage"] = developerMessage
        };

        // Set the response content back to the JSON string
        response.Content = CreateJsonContent(finalResponseContent.ToString());

        // Return the response
        return response;
    }

    private bool IsBinary(string content)
    {
        // Check if the content contains non-printable characters
        foreach (char c in content)
        {
            if (char.IsControl(c) && c != '\r' && c != '\n' && c != '\t')
            {
                return true;
            }
        }
        return false;
    }
}