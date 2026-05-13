using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Maru_Mod
{
    public static class ModUtil // 諸々はここに
    {
        public static string packageId = "Maru_Mod"; // パッケージId
        public static string path;
        public static Dictionary<string, Sprite> ArtWorks = new Dictionary<string, Sprite>();
    }
    public static class BufUtil // バフに関する処理を追加する
    {
        /// <summary>
        /// 指定スタック数のバフを付与する。既に付与されていれば加算する。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="stack"></param>
        public static void AddBufOrAddStack<T>(this BattleUnitBufListDetail self, int stack) where T : BattleUnitBuf, new()
        {
            T buf = self.GetBuf<T>();

            if (buf == null)
            {
                T customBuf = new T();
                customBuf.stack = stack;
                self.AddBuf(customBuf);
                customBuf.OnAddBuf(stack);
            }
            else
            {
                buf.stack += stack;
                buf.OnAddBuf(stack);
            }
        }

        /// <summary>
        /// 指定スタック数のバフを次の幕に付与する。既に付与されていれば加算する。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="stack"></param>
        public static void AddReadyBufOrAddStack<T>(this BattleUnitBufListDetail self, int stack) where T : BattleUnitBuf, new()
        {
            T buf = self.GetReadyBuf<T>();

            if (buf == null)
            {
                T customBuf = new T();
                customBuf.stack = stack;
                self.AddReadyBuf(customBuf);
                customBuf.OnAddBuf(stack);
            }
            else
            {
                buf.stack += stack;
                buf.OnAddBuf(stack);
            }
        }

        /// <summary>
        /// バフのスタック数を取得する。取得できない場合は既定値を返す
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="orElse"></param>
        /// <returns></returns>
        public static int GetStackOrElse<T>(this BattleUnitBufListDetail self, int orElse = 0) where T : BattleUnitBuf
        {
            T battleUnitBuf = self.GetBuf<T>();

            return battleUnitBuf?.stack ?? orElse;
        }

        /// <summary>
        /// 次の幕のバフのスタック数を取得する。取得できない場合は既定値を返す
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="orElse"></param>
        /// <returns></returns>
        public static int GetReadyStackOrElse<T>(this BattleUnitBufListDetail self, int orElse = 0) where T : BattleUnitBuf
        {
            T battleUnitBuf = self.GetReadyBuf<T>();

            return battleUnitBuf?.stack ?? orElse;
        }

        /// <summary>
        /// バフを取得する
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <returns></returns>
        public static T GetBuf<T>(this BattleUnitBufListDetail self) where T : BattleUnitBuf
        {
            T battleUnitBuf = self.GetActivatedBufList().Find(x => x is T && !x.IsDestroyed()) as T;

            return battleUnitBuf;
        }

        /// <summary>
        /// 次の幕のバフを取得する
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <returns></returns>
        public static T GetReadyBuf<T>(this BattleUnitBufListDetail self) where T : BattleUnitBuf
        {
            T battleUnitBuf = self.GetReadyBufList().Find(x => x is T && !x.IsDestroyed()) as T;

            return battleUnitBuf;
        }
    }
}
