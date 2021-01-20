using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class ThreadTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
        //ThreadManager.Instance.Start();
        int n = 10001;
        Stopwatch time = new Stopwatch();
        time.Start();
        int[] arr = GetRandomSequence2(10001, 10000);
        QuickSort(arr, 0, arr.Length - 1);
        UnityEngine.Debug.Log(test(n));
        time.Stop();

        UnityEngine.Debug.Log(string.Format("quick sort 耗时：{0} 毫秒", time.ElapsedMilliseconds) );

        //time.Start();
        //QuickSort(arr,0,arr.Length-1);
        //UnityEngine.Debug.Log(arr);
        //time.Stop();
        //UnityEngine.Debug.Log(string.Format("quick sort 耗时：{0} 毫秒", time.ElapsedMilliseconds));


    }

    private int fib(int n)
    {
        if (n < 2)
            return n;
        else
            return fib(n - 2) + fib(n - 1);
    }
    //1-2+3-4+……+10001
    private int test(int m)
    {
        int num = 0;
        for(int i=1;i<=m;i++)
        {
            if (i % 2 > 0)
                num += 1;
            else
                num -= 1;
        }
        return num;
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.D) && Input.GetKey(KeyCode.LeftControl))
        //{
        //    ThreadManager.Instance.CloseThread();
        //}

        //if (Input.GetKeyDown(KeyCode.A) && Input.GetKey(KeyCode.LeftControl))
        //{
        //    ThreadManager.Instance.AddEvent(TestEvent);
        //}
    }

    private void TestEvent()
    {
        //for(int i=0;i<10;i++)
        //{
        //    UnityEngine.Debug.Log("+++ : " + i);
        //}
        //UnityEngine.Debug.Log("testEvent");
    }

    private void OnDestroy()
    {
        //ThreadManager.Instance.CloseThread();
    }


    public int[] GetRandomSequence2(int total, int n)
    {
        //随机总数组
        int[] sequence = new int[total];
        //取到的不重复数字的数组长度
        int[] output = new int[n];
        for (int i = 0; i < total; i++)
        {
            sequence[i] = i;
        }
        int end = total - 1;
        for (int i = 0; i < n; i++)
        {
            //随机一个数，每随机一次，随机区间-1
            int num = UnityEngine.Random.Range(0, end + 1);
            output[i] = sequence[num];
            //将区间最后一个数赋值到取到数上
            sequence[num] = sequence[end];
            end--;
            //执行一次效果如：1，2，3，4，5 取到2
            //则下次随机区间变为1,5,3,4;
        }
        return output;
    }

    private void QuickSort(int[] arr , int Low, int High)
    {
        if (Low >= High) return;
        int low = Low;
        int high = High;
        int temp = arr[low];
        while(low < high)
        {
            while(low < high && arr[high] >= temp)
            {
                high--;
            }
            arr[low] = arr[high];
            while(low < high && arr[low] <= temp)
            {
                low++;
            }
            arr[high] = arr[low];
        }
        arr[high] = temp;
        QuickSort(arr, low, high - 1);
        QuickSort(arr, low + 1, high);
    }

    public void MergeSort(int[] arr)
    {
        int[] tempArray = new int[arr.Length];
        RecMergeSort(arr,tempArray, 0, arr.Length - 1);
    }

    public void RecMergeSort(int[] arr , int[] tempArray, int lbound, int ubound)
    {
        if (lbound == ubound)
            return;
        else
        {
            //求出中间的下标
            int mid = (int)(lbound + ubound) / 2;
            //子集1的递归
            RecMergeSort(arr,tempArray, lbound, mid);
            //子集2的递归
            RecMergeSort(arr,tempArray, mid + 1, ubound);
            //合并子集1和子集2
            Merge(arr,tempArray, lbound, mid + 1, ubound);
        }
    }

    public void Merge(int[] arr , int[] tempArray, int lowp, int highp, int ubound)
    {
        //主数组arr的初始下标位置
        int lbound = lowp;
        //子集1的长度
        int mid = highp - 1;
        //主数组arr的长度
        int n = (ubound - lbound) + 1;
 
        int j = 0;
        //3个while的作用是合并子集1和子集2到临时的数组中
        while ((lowp <= mid) && (highp <= ubound))
        {
            if (arr[lowp] < arr[highp])
            {
                tempArray[j] = arr[lowp];
                lowp++;
                j++;
            }
            else
            {
                tempArray[j] = arr[highp];
                highp++;
                j++;
            }
        }
        //子集1还留有数值时
        while (lowp <= mid)
        {
            tempArray[j] = arr[lowp];
            j++;
            lowp++;
        }
        //子集2还留有数值时
        while (highp <= ubound)
        {
            tempArray[j] = arr[highp];
            j++;
            highp++;
        }
 
        //将临时数组存储的数值替换主数组的数值。
        for (j = 0; j <= n - 1; j++)
        {
            arr[lbound + j] = tempArray[j];
        }
    }

}
