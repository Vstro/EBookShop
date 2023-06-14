using System;
using System.Collections.Generic;

namespace BookHaven.Models
{
    public class ReadingChapterViewModel
    {
        public Chapter CurrentChapter { get; set; }
        public Guid PrevChapterId { get; set; }
        public Guid NextChapterId { get; set; }
        public List<String> LikedUsersIds { get; set; }
    }
}
