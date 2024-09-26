## Usage

# ShopifyAIFunction
This project demonstrates how to use Generative AI to rewrite product descriptions in a Shopify store using Azure Functions.
## Overview
The `ShopifyAIFunction` project leverages OpenAI's GPT-3 to generate high-quality, engaging product descriptions. By integrating with Shopify's API, the application fetches existing product descriptions, processes them through the AI model, and updates the store with the new descriptions.
## Features

- **Fetch Product Descriptions:** Retrieve existing product descriptions from your Shopify store.
- **Generate New Descriptions:** Use Generative AI to create engaging and unique product descriptions.
- **Update Store:** Automatically update your Shopify store with the newly generated descriptions.
- **Logging and Monitoring:** Keep track of the changes and monitor the performance of the AI model.

# WordpressBlogAIFunction
Welcome to the **WordpressBlogAIFunction** project! This innovative project demonstrates how to read the latest questions from Reddit, use those questions as content seeds to generate blog posts using Generative AI, and post the generated content to a WordPress blog using API Key and API Secret.
## Overview
Imagine turning the latest Reddit questions into engaging blog posts effortlessly. That's exactly what the `WordpressBlogAIFunction` project does! By leveraging OpenAI's GPT-3, this project generates high-quality blog posts based on the freshest questions from Reddit. The application fetches these questions, processes them through the AI model to create compelling content, and then posts the content directly to your WordPress blog using the WordPress API. It's like having a smart content generator at your fingertips!
## Features
- **Fetch Latest Questions from Reddit:** Automatically retrieve the latest questions from your favorite subreddits.
- **Generate Blog Posts:** Use Generative AI to craft engaging and unique blog posts based on the fetched questions.
- **Post to WordPress:** Seamlessly post the generated content to your WordPress blog.
- **Logging and Monitoring:** Keep track of all activities and monitor the performance of the AI model.

 **Configure Environment Variables:**
   Create a `local.settings.json` file in the root directory and add your configuration settings:
   ```json
    {
      "Values": {
        "AzureWebJobsStorage": "your_azure_storage_connection_string",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet",
        "REDDIT_API_KEY": "your_reddit_api_key",
        "REDDIT_API_SECRET": "your_reddit_api_secret",
        "WORDPRESS_API_KEY": "your_wordpress_api_key",
        "WORDPRESS_API_SECRET": "your_wordpress_api_secret"
      }
    }
