﻿namespace ReCommand.API.Dtos
{
    public class Contact
    {
        public Contact()
        {
            Tags = new string[] { };
        }
        /// <summary>
        /// 用户Id
        /// </summary>
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Company { get; set; }
        public string Title { get; set; }
        public string Phone { get; set; }
        public string Avatar { get; set; }

        public string[] Tags { get; set; }
    }
}