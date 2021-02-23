using UnityEngine;

namespace GStore
{
    /// <summary>
    /// 工具类，存放一些通用的静态函数
    /// </summary>
    public static class DataUtil
    {
        #region 位操作相关
        /// <summary>
        /// 检查标识位
        /// </summary>
        /// <param name="state"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        public static bool IsBit(int state, int flag)
        {
            return (state & flag) != 0;
        }
        /// <summary>
        /// 设置标识位
        /// </summary>
        /// <param name="state"></param>
        /// <param name="flag"></param>
        /// <param name="value"></param>
        public static void SetBit(ref int state, int flag, bool value)
        {
            if (value)
            {
                SetBit(ref state, flag);
            }
            else
            {
                ClearBit(ref state, flag);
            }
        }
        /// <summary>
        /// 设置标识位
        /// </summary>
        /// <param name="state"></param>
        /// <param name="flag"></param>
        public static void SetBit(ref int state, int flag)
        {
            state |= flag;
        }
        /// <summary>
        /// 清除标识位
        /// </summary>
        /// <param name="state"></param>
        /// <param name="flag"></param>
        public static void ClearBit(ref int state, int flag)
        {
            state &= ~flag;
        }
        #endregion

        /// <summary>
        /// 最大值
        /// </summary>
        /// <param name="_num1"></param>
        /// <param name="_num2"></param>
        /// <returns></returns>
        public static int Max(int _num1, int _num2)
        {
            return _num1 > _num2 ? _num1 : _num2;
        }

        public static long Max(long _num1, long _num2)
        {
            return _num1 > _num2 ? _num1 : _num2;
        }

        public static float Max(float _num1, float _num2)
        {
            return _num1 > _num2 ? _num1 : _num2;
        }

        public static int Max(int val0, int val1, int val2, int val3)
        {
            if (val1 > val0)
            {
                val0 = val1;
            }
            if (val2 > val0)
            {
                val0 = val2;
            }
            if (val3 > val0)
            {
                val0 = val3;
            }
            return val0;
        }
        /// <summary>
        /// 最小值
        /// </summary>
        /// <param name="_num1"></param>
        /// <param name="_num2"></param>
        /// <returns></returns>
        public static int Min(int _num1, int _num2)
        {
            return _num1 < _num2 ? _num1 : _num2;
        }

        /// <summary>
        /// 确保某值的最小最大范围
        /// </summary>
        /// <param name="_value"></param>
        /// <param name="_min"></param>
        /// <param name="_max"></param>
        /// <returns></returns>
        public static int Bound(int _value, int _min, int _max)
        {
            if (_value < _min)
            {
                _value = _min;
            }

            if (_value > _max)
            {
                _value = _max;
            }

            return _value;
        }

        /// <summary>
        /// 随机值
        /// </summary>
        /// <param name="_min"></param>
        /// <param name="_max"></param>
        /// <returns>[_min,_max) 即包含_min排除_max</returns>
        public static int Random(int _min, int _max)
        {
            //最大值不能比最小值小
            if (_max < _min)
            {
                Debug.LogError("");
                return _min;
            }
            return UnityEngine.Random.Range(_min, _max);
        }

        /// <summary>
        /// 随机取整
        /// </summary>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static int RandomInt(float rate)
        {
            return (int)(UnityEngine.Random.Range(0, 1f) + rate);
        }

        /// <summary>
        /// string to float
        /// </summary>
        /// <param name="_float_str"></param>
        /// <returns></returns>
        public static float StrToFloat(string _float_str, float _default_vale = 0)
        {
            if (string.IsNullOrEmpty(_float_str))
            {
                return _default_vale;
            }
            if (_float_str.Length > 0)
            {
                float _result;
                if (float.TryParse(_float_str, out _result))
                {
                    return _result;
                }
                else
                {
                    return _default_vale;
                }
            }
            else
            {
                return _default_vale;
            }
        }

        /// <summary>
        /// string to int
        /// </summary>
        /// <param name="_int_str"></param>
        /// <param name="_default_vale"></param>
        /// <returns></returns>
        public static int StrToInt(string _int_str, int _default_vale = -1)
        {
            if (string.IsNullOrEmpty(_int_str))
            {
                return _default_vale;
            }
            if (_int_str.Length > 0)
            {
                int _result;
                if (int.TryParse(_int_str, out _result))
                {
                    return _result;
                }
                else
                {
                    return _default_vale;
                }
            }
            else
            {
                return _default_vale;
            }
        }

        /// <summary>
        /// string to vector3 Like: "1,1,1"  -> new vector3(1,1,1)
        /// </summary>
        /// <param name="_vector_str"></param>
        /// <returns></returns>
        public static Vector3 StrToVector3(string _vector_str)
        {
            try
            {
                float[] _pos = new float[3];
                int _start_index = 0;
                int _end_index = 0;
                for (int i = 0; i < 3; i++)
                {
                    if (i == 2)
                    {
                        _end_index = _vector_str.Length;
                    }
                    else
                    {
                        _end_index = _vector_str.IndexOf(',', _start_index);
                    }
                    _pos[i] = StrToFloat(_vector_str.Substring(_start_index, _end_index - _start_index));
                    _start_index = _end_index + 1;
                }
                return new Vector3(_pos[0], _pos[1], _pos[2]);
            }
            catch
            {
                return Vector3.zero;
            }
        }
    }
}
