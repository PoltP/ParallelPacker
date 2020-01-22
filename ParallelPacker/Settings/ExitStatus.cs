using System;
namespace ParallelPacker.Settings {
    public enum ExitStatus {
       SUCCESS = 0,
       ERROR = 1,
       CANCEL = 2
    }

    public static class ExitStatusExtension {
        public static int ToInteger(this ExitStatus exitStatus) {
            return (int)exitStatus;
        }
    }
}
