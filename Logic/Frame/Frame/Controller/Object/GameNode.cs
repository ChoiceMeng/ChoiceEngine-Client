using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frame
{
    abstract class GameNode
    {
        public bool IsRunning
        {
            get;
            set;
        }
        /// <summary>
        ///   start
        /// </summary>
        public abstract bool Start();

        /// <summary>
        ///   init
        /// </summary>
        public abstract bool Init();

        /// <summary>
        ///   pause
        /// </summary>
        public abstract bool Pause();

        /// <summary>
        ///   stop
        /// </summary>
        public abstract bool Stop();

        /// <summary>
        ///   destroy
        /// </summary>
        public abstract bool Destroy();
    }
}
