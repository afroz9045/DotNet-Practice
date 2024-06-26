﻿namespace LibraryManagement.Core.Dtos
{
    public class BookDto
    {
        public int BookId { get; set; }
        public string BookName { get; set; } = null!;
        public int Isbn { get; set; }
        public string AuthorName { get; set; } = null!;
        public string BookEdition { get; set; } = null!;
        public int? StockAvailable { get; set; }

        public bool StockUpdate { get; set; } = false;
    }
}