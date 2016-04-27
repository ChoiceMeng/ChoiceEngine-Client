using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// This class is adapted and modified from the FSM implementation class available on UnifyCommunity website
/// The license for the code is Creative Commons Attribution Share Alike.
/// It's originally the port of C++ FSM implementation mentioned in Chapter01 of Game Programming Gems 1
/// You're free to use, modify and distribute the code in any projects including commercial ones.
/// Please read the link to know more about CCA license @http://creativecommons.org/licenses/by-sa/3.0/

/// This class represents the States in the Finite State System.
/// Each state has a Dictionary with pairs (transition-state) showing
/// which state the FSM should be if a transition is fired while this state
/// is the current state.
/// Reason method is used to determine which transition should be fired .
/// Act method has the code to perform the actions the NPC is supposed to do if it�s on this state.
/// </summary>
public abstract class FSMState
{
    //�ֵ䣬�ֵ���ÿһ���¼��һ����ת��-״̬���� ����Ϣ
    protected Dictionary<Transition, FSMStateID> map = new Dictionary<Transition, FSMStateID>();
    //״̬���ID
    protected FSMStateID stateID;
    public FSMStateID ID { get { return stateID; } }
    //������Ҫ�õ��ģ����״̬��صı���
    //Ŀ����λ��
    protected Vector3 destPos;
    //Ѳ�ߵ�����飬�洢Ѳ��Ҫ�����ĵ�
    protected Transform[] waypoints;
    //ת���ٶ�
    protected float curRotSpeed;
    //�ƶ��ٶ�
    protected float curSpeed;
    //AI����Ҿ���С�ڸ�ֵ����ʼ׷��
	protected float chaseDistance = 40.0f;
    //С������͹���
	protected float attackDistance = 20.0f;
    //Ѳ��С����ֵ������Ϊ����Ѳ�ߵ���
	protected float arriveDistance = 3.0f;

    /// <summary>
    /// ���ֵ�����ÿ����һ��"ת��--״̬"��
    /// </summary>
    /// <param name="transition"></param>
    /// <param name="id"></param>
    public void AddTransition(Transition transition, FSMStateID id)
    {
        //������ת��(���Կ������ֵ�Ĺؼ���)�Ƿ����ֵ���
        if (map.ContainsKey(transition))
        {
            //һ��ת��ֻ�ܶ�Ӧһ����״̬
            Debug.LogWarning("FSMState ERROR: transition is already inside the map");
            return;
        }
        //��������ֵ䣬��ô�����ת����ת�����״̬��Ϊһ���µ��ֵ�������ֵ�
        map.Add(transition, id);
        Debug.Log("Added : " + transition + " with ID : " + id);
    }

/// <summary>
/// ���ֵ���ɾ����
/// </summary>
/// <param name="trans"></param>
    public void DeleteTransition(Transition trans)
    {
        // ����Ƿ����ֵ��У�����ڣ��Ƴ�
        if (map.ContainsKey(trans))
        {
            map.Remove(trans);
            return;
        }
        //���Ҫɾ��������ֵ��У��������
        Debug.LogError("FSMState ERROR: Transition passed was not on this State List");
    }


/// <summary>
/// ͨ����ѯ�ֵ䣬ȷ���ڵ�ǰ״̬�£�����transת��ʱ��Ӧ��ת�����µ�״̬��Ų�����
/// </summary>
/// <param name="trans"></param>
/// <returns></returns>
    public FSMStateID GetOutputState(Transition trans)
    {
		return map[trans];
    }


    /// <summary>
    /// ����ȷ���Ƿ���Ҫת��������״̬��Ӧ�÷����ĸ�ת��
    /// </summary>
    /// <param name="player"></param>
    /// <param name="npc"></param>
    public abstract void Reason(Transform player, Transform npc);

/// <summary>
/// �������ڱ�״̬�Ľ�ɫ��Ϊ���ƶ���������
/// </summary>
/// <param name="player"></param>
/// <param name="npc"></param>
    public abstract void Act(Transform player, Transform npc);

    /// <summary>
    /// Ѱ����һ��Ѳ�ߵ�
    /// </summary>
    public void FindNextPoint()
    {
        //Debug.Log("Finding next point");
        int rndIndex = Random.Range(0, waypoints.Length);
        Vector3 rndPosition = Vector3.zero;
        destPos = waypoints[rndIndex].position + rndPosition;
    }

    /// <summary>
    /// Check whether the next random position is the same as current tank position
    /// </summary>
    /// <param name="pos">position to check</param>
    /*
	protected bool IsInCurrentRange(Transform trans, Vector3 pos)
    {
        float xPos = Mathf.Abs(pos.x - trans.position.x);
        float zPos = Mathf.Abs(pos.z - trans.position.z);

        if (xPos <= 50 && zPos <= 50)
            return true;

        return false;
    }*/
}
