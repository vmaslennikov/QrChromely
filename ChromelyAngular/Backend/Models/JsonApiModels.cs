using System;
using System.Collections.Generic;
using System.Text;

namespace ChromelyAngular.Backend.Models.JsonApi
{
    public abstract class BaseJsonObject
    {
        public string Id { get; set; }

        public DateTime? Created { get; set; }

        public DateTime? Modified { get; set; }
    }

    public abstract class BaseNameObject : BaseJsonObject
    {
        public string Name { get; set; }
    }

    /// <summary>
    /// Должности
    /// </summary>
    public class Position : BaseNameObject
    {
        public int Count { get; set; }
    }
    /// <summary>
    /// Зона
    /// </summary>
    public class Area : BaseNameObject
    {
        public string Code { get; set; }
    }

    /// <summary>
    /// Стадион
    /// </summary>
    public class Place : BaseNameObject
    {
        public long TimeZone { get; set; }
    }

    /// <summary>
    ///  Матчи
    /// </summary>
    public class Event : BaseNameObject
    {
        public DateTime Date { get; set; }

        public Place Place { get; set; }
        public int PlaceId { get; set; }

        public string HostName { get; set; }

        public string GuestName { get; set; }

        public int? RplId { get; set; }

        public int? RplPlaceId { get; set; }

        public string RplPlaceName { get; set; }

        public DateTime? RplDateTime { get; set; }

        public int? TourNum { get; set; }

        public int? TournamentId { get; set; }

        public Tournament Tournament { get; set; }
    }

    public class BlockReason : BaseNameObject
    {
        //public int Duration { get; set; }
    }

    /// <summary>
    /// Соревнования
    /// </summary>
    public class Tournament : BaseNameObject
    {
        public string PassTemplate { get; set; }
    }

}

