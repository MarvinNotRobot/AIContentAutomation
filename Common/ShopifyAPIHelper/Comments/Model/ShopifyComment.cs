namespace Common.ShopifyAPI.Comments.Model
{
    /// <summary>
    /// Represents a product review in the Shopify store.
    /// </summary>
    class ProductReview
    {
        /// <summary>
        /// Gets or sets the handle of the product being reviewed.
        /// </summary>
        public string ProductHandle { get; set; }

        /// <summary>
        /// Gets or sets the state of the review (e.g., published, pending).
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the rating given in the review.
        /// </summary>
        public int Rating { get; set; }

        /// <summary>
        /// Gets or sets the title of the review.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the author of the review.
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// Gets or sets the email of the author.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the location of the author.
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Gets or sets the body content of the review.
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Gets or sets the reply to the review.
        /// </summary>
        public string Reply { get; set; }

        /// <summary>
        /// Gets or sets the creation date of the review.
        /// </summary>
        public string CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the date when the review was replied to.
        /// </summary>
        public string RepliedAt { get; set; }
    }
}
