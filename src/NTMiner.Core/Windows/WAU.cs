﻿using System;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace NTMiner.Windows {
    public static class WAU {
        public static void DisableWAUAsync() {
            Task.Factory.StartNew(() => {
                ServiceController sc = new ServiceController("wuauserv");
                try {
                    if (sc != null && sc.Status == ServiceControllerStatus.Running) {
                        sc.Stop();
                        sc.WaitForStatus(ServiceControllerStatus.Stopped);
                        Global.Logger.OkDebugLine("Windows自动更新禁用成功");
                    }
                    else {
                        Global.Logger.OkDebugLine("Windows自动更新已经处于禁用状态，无需再次禁用");
                    }
                    sc.Close();
                }
                catch (Exception ex) {
                    Global.Logger.ErrorDebugLine("Windows自动更新禁用失败，因为异常", ex);
                }
            });
        }
    }
}
