using System;
using ProtoBuf;

namespace LegendsOfDescent.Analytics
{
    [ProtoContract]
    public class ExceptionInfo
    {
        [ProtoMember(1)]
        public string Type { get; set; }

        [ProtoMember(2)]
        public string Message { get; set; }

        [ProtoMember(3)]
        public string StackTrace { get; set; }

        [ProtoMember(4)]
        public ExceptionInfo InnerException { get; set; }

        public ExceptionInfo()
        {
        }

        public ExceptionInfo(Exception e)
        {
            if (e == null) return;

            Type = e.GetType().ToString();
            Message = e.Message;
            StackTrace = e.StackTrace;

            if (e.InnerException != null)
            {
                InnerException = new ExceptionInfo(e.InnerException);
            }
        }
    }
}
