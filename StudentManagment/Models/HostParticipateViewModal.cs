namespace StudentManagment.Models
{
    public class Hosts
    {
        public int HostId { get; set; }
        public string AspNetUserId { get; set; }

        public string HostName { get; set; }

        public bool IsStarted { get; set; }
    }

    public class Participants
    {
        public int ParticipantId { get; set; }

        public string ParticipantName { get; set; }

        public string AspNetUserId { get; set; }

        public int HostId { get; set; }

        public string ParticipantIdString { get; set; }
    }

    public class HostParticipateViewModal
    {
        public int HostId { get; set; }

        public int ParticipateId { get; set; }

        public List<string> HostClientIds { get; set; }
    }
    public class Call
    {
        public string HostId { get; set; }

        public string AspNetUserId { set; get; }
        public List<string> Participants { get; set; } = new List<string>();
    }
}
