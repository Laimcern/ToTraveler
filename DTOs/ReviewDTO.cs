﻿using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ToTraveler.DTOs
{
    public class ReviewDTO_Put
    {

        [Range(1, 5)]
        public int Rating { get; set; }
        public string? Text { get; set; }
        public bool IsPrivate { get; set; }
    }
}
