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

//����ö�٣�Ϊ���ܵ�ת��������
public enum Transition
{    
    SawPlayer = 0, //�������
    ReachPlayer,   //�ӽ����
    LostPlayer,    //����뿪����
    NoHealth,      //����
}

/// <summary>
/// ����ö�٣�Ϊ���ܵ�״̬������ID
/// </summary>
public enum FSMStateID
{    
    Patrolling = 0,  //Ѳ�߱��
    Chasing,         //׷����
    Attacking,       //�������
    Dead,           //�������
}

public class AdvancedFSM : FSM 
{
    //FSM�е�����״̬��ɵ��б�
    private List<FSMState> fsmStates;
    //��ǰ״̬�ı��
    //The fsmStates are not changing directly but updated by using transitions
    private FSMStateID currentStateID;
    public FSMStateID CurrentStateID { get { return currentStateID; } }
    //��ǰ״̬
    private FSMState currentState;
    public FSMState CurrentState { get { return currentState; } }

    public AdvancedFSM()
    {
        //�½�һ���յ�״̬�б�
        fsmStates = new List<FSMState>();
    }

    /// <summary>
    ///��״̬�б��м���һ���µ�״̬
    /// </summary>
    public void AddFSMState(FSMState fsmState)
    {
        //���Ҫ�������״̬�Ƿ�Ϊ�գ�����վͱ���
        if (fsmState == null)
        {
            Debug.LogError("FSM ERROR: Null reference is not allowed");
        }

        // First State inserted is also the Initial state
        //   the state the machine is in when the simulation begins
        //�����������״̬ʱ���б��ǿյģ���ô���������б�����
        if (fsmStates.Count == 0)
        {
            fsmStates.Add(fsmState);
            currentState = fsmState;
            currentStateID = fsmState.ID;
            return;
        }

        // ���Ҫ�����״̬�Ƿ��Ѿ����б������ǣ�������
        foreach (FSMState state in fsmStates)
        {
            if (state.ID == fsmState.ID)
            {
                Debug.LogError("FSM ERROR: Trying to add a state that was already inside the list");
                return;
            }
        }
        //���Ҫ�����״̬�����б��У����������б�
        fsmStates.Add(fsmState);
    }

    
    //��״̬��ɾ��һ��״̬   
    public void DeleteState(FSMStateID fsmState)
    {
        // ��������״̬�б����Ҫɾ����״̬���б��У���ô�����Ƴ������򱨴�
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
    /// ���ݵ�ǰ״̬���Ͳ����д��ݵ�ת����ת������״̬
    /// </summary>
    public void PerformTransition(Transition trans)
    {  
        // ������ǰ��״̬�࣬��TransΪ������������GetOutputState����
        //ȷ��ת�������״̬
        FSMStateID id = currentState.GetOutputState(trans);        

        //  ����ǰ״̬�������Ϊ�ոշ��ص���״̬���	
        currentStateID = id;
        //����״̬��Ų���״̬�б�����ǰ״̬����Ϊ���ҵ���״̬
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
