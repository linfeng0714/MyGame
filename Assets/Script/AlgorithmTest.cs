using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlgorithmTest : MonoBehaviour
{
    public class ListNode
    {
        public int val;
        public ListNode next;
        public ListNode(int x) { val = x; }
    }
    // Start is called before the first frame update
    void Start()
    {
        int[] arr = { 0, 5, 9, 13, 23, 5, 3, 5, 78, 67, 65, 89, 0, 3, 1, 17, 18, 21 };
        int left = 0;
        int right = arr.Length-1;
        int[] arrA = quickSort(arr, left, right);
        TwoSum();
    }
    /// <summary>
    /// 解法： 9-num[i]存入字典【key=num[i],value=i】,依次对比9-num[i],查看字典中是否有这个KEY值
    /// </summary>
    /// <returns></returns>
    public int[] TwoSum()
    {
        int[] nums = { 2, 4, 11, 15 , 7 };
        int target = 9;
        Dictionary<int, int> dict = new Dictionary<int, int>();
        for(int i=0;i<nums.Length;i++)
        {
            int r = target - nums[i];
            if (dict.ContainsKey(r))
            {
                int[] result = new int[] { dict[r], i };
                return result;
            } 
            else if(!dict.ContainsKey(nums[i]))
            {
                dict.Add(nums[i], i);
            }
        }
        return new int[] { 0,0};
    }

    public void AddTwoNumbers()
    {
        ListNode l1 = new ListNode(8);
        l1.next = new ListNode(9);
        l1.next.next = new ListNode(7);
        ListNode l2 = new ListNode(3);
        l1.next = new ListNode(4);
    }

    public int[] quickSort(int[] arr,int left , int right)
    {
        
        if (left >= right)
            return null;
        int i = left;
        int j = right;
        int val = arr[i];//传进来的第一个数
        int temp = 0;
        while(true)
        {
            for(;j>i;--j)
            {
                if (i == j || arr[j] < val)
                    break;
            }
            for (; i < j; ++i)
            {
                if (i == j || arr[i] > val)
                    break;
            }
            if(i==j)
            {
                arr[left] = arr[i];
                arr[i] = val;
                quickSort(arr, left, i - 1);
                quickSort(arr, i+1, right);
                return arr;
            }
            else
            {
                temp = arr[i];
                arr[i] = arr[j];
                arr[j] = temp;
            }
        }

        return arr;
    }
}
