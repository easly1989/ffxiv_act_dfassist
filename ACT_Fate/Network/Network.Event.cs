using System;


namespace App
{
    internal partial class Network
    {
        public enum EventType
        {
            NONE,
            INSTANCE_ENTER,     // [0]=instance code
            INSTANCE_EXIT,      // [0]=instance code

            FATE_BEGIN,         // [0]=fate code
            FATE_PROGRESS,      // [0]=fate code, [1]=progress
            FATE_END,           // [0]=fate code, [1]=status(?)

            //유저가 매칭 신청 
            MATCH_BEGIN,        // [0]=match type(0,1), [1]=roulette code or ins count, [...]=ins
            //매칭 상태
            MATCH_PROGRESS,     // [0]=instance code, [1]=status, [2]=tank, [3]=healer, [4]=dps
            //매칭됨
            MATCH_ALERT,        // [0]=roulette code, [1]=instance code
            //매칭 종료
            MATCH_END,          // [0]=end reason (MatchEndType)
        }

        public enum MatchType
        {
            ROULETTE = 0,
            SELECTIVE = 1
        }
        public enum MatchEndType
        {
            CANCELLED = 0,
            ENTER_INSTANCE = 1
        }

        public delegate void EventHandler(int pid, EventType eventType, int[] args);

        public event EventHandler onReceiveEvent;
            
        private void fireEvent(EventType eventType, int[] args)
        {
            if (onReceiveEvent != null) onReceiveEvent(pid, eventType, args);
        }
    }
}
