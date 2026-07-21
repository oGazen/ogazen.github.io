using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using System;

public class WaitEditor
{
    public class Milliseconds : CustomYieldInstruction
    {
        private bool isFinish = true;
        private DateTime signTime;
        private int intervalTime_Milliseconds;
        public override bool keepWaiting
        {
            get
            {
                if (isFinish)
                {
                    isFinish = false;
                    signTime = DateTime.Now;
                }

                bool isSuccess = DateTime.Now.Subtract(signTime).TotalMilliseconds < intervalTime_Milliseconds;
                if (!isSuccess)
                    isFinish = true;
                return isSuccess;
            }
        }

        public Milliseconds(int s)
        {
            this.Refresh_IntervalTime(s);
        }

        // 更新等待间隔
        public void Refresh_IntervalTime(int s)
        {
            intervalTime_Milliseconds = s;
        }


    }



}
