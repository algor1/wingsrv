using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


//три таблицы 
//skillsDB - список всех скилов
//playerSkills - список изученных скилов
//playerSkillQueue - список изучаемых  сейчас скилов очередь до 3-х дней, мах 10 скилов

    
class SkillsServer
{
	public bool started;
    private Game gamePlugin;
    public Login _loginPlugin;
    public DatabaseProxy _database { get; set; }


  
    public SkillsServer(Game game)
    {
        gamePlugin = game;
    }
    
    public void RunServer()
    {
		started = true;
        LearnTick();

    }


    public long GetSkillPoints(int playerId,int skillId,int tech)
    {
        long retPoints;
        try
        {
            _database.DataLayer.GetSkillPoints(playerId,skillId,tech, p => 
            {
                retPoints = p;
                //if (_debug)
                //   {
                //gamePlugin.WriteToLog("Ships loaded count:" + shipList.Count, DarkRift.LogType.Info);
                //   }
            });
        }
        catch (Exception ex)
        {
            gamePlugin.WriteToLog("Database error on gelting skill points" + ex, DarkRift.LogType.Error);

            //Return Error 2 for Database error
            _database.DatabaseError(null, 0, ex);
        }
        return retPoints;
    }

    public Skill GetSkill(int skillId)
    {
        Skill retSkill;
        try
        {
            _database.DataLayer.GetSkill(skillId, skill =>
            {
                retSkill = skill;
                //if (_debug)
                //   {
                //gamePlugin.WriteToLog("Ships loaded count:" + shipList.Count, DarkRift.LogType.Info);
                //   }
            });
        }
        catch (Exception ex)
        {
            gamePlugin.WriteToLog("Database error on gelting skill " + ex, DarkRift.LogType.Error);

            //Return Error 2 for Database error
            _database.DatabaseError(null, 0, ex);
        }
        
        return retSkill;
    }

	public List<SkillQueue> GetPlayerSkillQueue(int playerId)
    {
        List<SkillQueue> retSkillQueue;
        try
        {
            _database.DataLayer.GetPlayerSkillQueue(playerId, skillQueue =>
            {
                retSkillQueue = skillQueue;
                //if (_debug)
                //   {
                //gamePlugin.WriteToLog("Ships loaded count:" + shipList.Count, DarkRift.LogType.Info);
                //   }
            });
        }
        catch (Exception ex)
        {
            gamePlugin.WriteToLog("Database error on gelting skill " + ex, DarkRift.LogType.Error);

            //Return Error 2 for Database error
            _database.DatabaseError(null, 0, ex);
        }
        return retSkillQueue;
    }


	public bool AddSkillToQueue(int playerId, int skillId, int tech, int level, int queueNum)
    {
        List<SkillQueue> _PSQ = GetPlayerSkillQueue(player_id);

        try
        {
            _database.DataLayer.AddQueueSkill(playerId, skillId, tech, level, queueNum, ()  =>
            {
                //if (_debug)
                //   {
                gamePlugin.WriteToLog("Skill Added to queue:" + playerId+":"+skillId+":"+tech+":"+level , DarkRift.LogType.Info);
                //   }
            });
        }
        catch (Exception ex)
        {
            gamePlugin.WriteToLog("Database error on gelting skill " + ex, DarkRift.LogType.Error);

            //Return Error 2 for Database error
            _database.DatabaseError(null, 0, ex);
        }
        return retSkillQueue;
        
        
        
        
        
        
        for (int i = queueNum; i < _PSQ.Count; i++)
        {
            

			if (_PSQ[i].skill_id == skill_id && _PSQ[i].tech == _tech && _PSQ[i].level == level)
            {
                return false;
            }
        }
        GetComponent<SkillsDB>().AddToQueue(player_id, skill_id, _tech, level);
        return true;
    }

	public bool DeleteSkillFromQueue(int player_id, int skill_id, int _tech, int level)
    {
        List<SkillQueue> _PSQ = PlayerSkillQueue(player_id);
        for (int i = 0; i < _PSQ.Count; i++)
        {
			if (_PSQ[i].skill_id == skill_id && _PSQ[i].tech == _tech &&_PSQ[i].level==level)
            {
				GetComponent<SkillsDB>().DeleteFromQueue(player_id, skill_id, _tech,level);
                return true;
            }
        }
        return false;
    }

    public int SkillLevelCalc(int _tech, int _difficulty, long _points)
    {
        int level= Mathf.FloorToInt(Mathf.Log(_points, _difficulty*3+_tech-1));
        return level;
    }

    private void CheckDepSkill(int player_id,int skill_id,int _tech)
    {

    }

	private void AddPoints(int player_id, int pointsAdd)
    {
		Skill _skill = SkillFind (skillq.skill_id);
		long _points = SkillLearned (player_id, skillq.skill_id, skillq.tech);


			GetComponent<SkillsDB>().AddPointsToSkill(0, _skill.id, _skill.tech, pointsAdd);
		
		if (skillq.level<=SkillLevelCalc(skillq.tech,_skill.difficulty,_points)){
			GetComponent<SkillsDB> ().DeleteFromQueue (0, skillq.skill_id, skillq.tech, skillq.level);
		}
    }

    private IEnumerator LearnTick()
	{
		while (true) {
            //foreach player_id
                int playerLearningSpeed = 1;

                List<SkillQueue> _PSQ = PlayerSkillQueue(0); //пока только один плеер
                for (int i = 0; i < _PSQ.Count; i++)
				{
				if (_PSQ[i].queue==1) UpdatePlayerSkill (0,_PSQ[i],playerLearningSpeed);

                }
			yield return new WaitForSeconds (10f);
	
		}
    }
}

//void .GetSkillPoints(int playerId, int skillId, int tech, Action<long> callback);
//void GetSkill(skillId,, Action<Skill> callback);
//void GetPlayerSkillQueue(int playerId, Action<List<SkillQueue>> callback);
//void AddQueueSkill(int playerId, int skillId, int tech, int level, int queueNum, Action callback);
        //MoveDownQueue(int playerId, int queueNum)
        //Insert 

//private void MoveDownQueue(int playerId, int queueNum)
        //update set queue= queue+1 where queue>queueNum
        //delete where queue>10
//void DeleteQueueSkill(int playerId, int queueNum, Action callback);
        //delete where queue == queueNum
        //update set queue= queue-1 where queue>queueNum
//void AddPointsToSkill(int playerId, Action callback);
