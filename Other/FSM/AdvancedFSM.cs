using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// This class is adapted and modified from the FSM implementation class available on UnifyCommunity website
/// The license for the code is Creative Commons Attribution Share Alike.
/// It's originally the port of C++ FSM implementation mentioned in Chapter01 of Game Programming Gems 1
/// You're free to use, modify and distribute the code in any projects including commercial ones.
/// Please read the link to know more about CCA license @http://creativecommons.org/licenses/by-sa/3.0/
/// </summary>

//定义枚举，为可能的转换分配编号
public enum Transition
{    
    SawPlayer = 0, //看到玩家
    ReachPlayer,   //接近玩家
    LostPlayer,    //玩家离开视线
    NoHealth,      //死亡
}

/// <summary>
/// 定义枚举，为可能的状态分配编号ID
/// </summary>
public enum FSMStateID
{    
    Patrolling = 0,  //巡逻编号
    Chasing,         //追逐编号
    Attacking,       //攻击编号
    Dead,           //死亡编号
}

public class AdvancedFSM : FSM 
{
    //FSM中的所有状态组成的列表
    private List<FSMState> fsmStates;
    //当前状态的编号
    //The fsmStates are not changing directly but updated by using transitions
    private FSMStateID currentStateID;
    public FSMStateID CurrentStateID { get { return currentStateID; } }
    //当前状态
    private FSMState currentState;
    public FSMState CurrentState { get { return currentState; } }

    public AdvancedFSM()
    {
        //新建一个空的状态列表
        fsmStates = new List<FSMState>();
    }

    /// <summary>
    ///向状态列表中加入一个新的状态
    /// </summary>
    public void AddFSMState(FSMState fsmState)
    {
        //检查要加入的新状态是否为空，如果空就报错
        if (fsmState == null)
        {
            Debug.LogError("FSM ERROR: Null reference is not allowed");
        }

        // First State inserted is also the Initial state
        //   the state the machine is in when the simulation begins
        //如果插入的这个状态时，列表还是空的，那么将它加入列表并返回
        if (fsmStates.Count == 0)
        {
            fsmStates.Add(fsmState);
            currentState = fsmState;
            currentStateID = fsmState.ID;
            return;
        }

        // 检查要加入的状态是否已经在列表里，如果是，报错返回
        foreach (FSMState state in fsmStates)
        {
            if (state.ID == fsmState.ID)
            {
                Debug.LogError("FSM ERROR: Trying to add a state that was already inside the list");
                return;
            }
        }
        //如果要加入的状态不在列表中，将它加入列表
        fsmStates.Add(fsmState);
    }

    
    //从状态中删除一个状态   
    public void DeleteState(FSMStateID fsmState)
    {
        // 搜索整个状态列表，如果要删除的状态在列表中，那么将它移除，否则报错
        foreach (FSMState state in fsmStates)
        {
            if (state.ID == fsmState)
            {
                fsmStates.Remove(state);
                return;
            }
        }
        Debug.LogError("FSM ERROR: The state passed was not on the list. Impossible to delete it");
    }

    /// <summary>
    /// 根据当前状态，和参数中传递的转换，转换到新状态
    /// </summary>
    public void PerformTransition(Transition trans)
    {  
        // 根绝当前的状态类，以Trans为参数调用它的GetOutputState方法
        //确定转换后的新状态
        FSMStateID id = currentState.GetOutputState(trans);        

        //  将当前状态编号设置为刚刚返回的新状态编号	
        currentStateID = id;
        //根绝状态编号查找状态列表，将当前状态设置为查找到的状态
        foreach (FSMState state in fsmStates)
        {
            if (state.ID == currentStateID)
            {
                currentState = state;
                break;
            }
        }
    }
}
