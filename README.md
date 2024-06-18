# AI-Empowered-Robotics (Active Research and Ongoing Projects)
Keywords: Reinforcement Learning, Imitation Learning, Digital Twins, and Robotics. 

Simulation: Unity

Tool: ML-Agents

# Dynamic Environment Generation & Navigation Agent (Unity)
![Dynamic Room Generator](https://github.com/duyhho/AI-Empowered-Robotics/blob/main/Media/dynamic_room_generator.gif?raw=true)
## Collaborative Firefighting Agent
![Collaborative Firefighting Agent](https://github.com/duyhho/AI-Empowered-Robotics/blob/main/Media/Collab%20Fire%20Agent.gif)

### Task Description:
The agent's task is to navigate a multi-room environment to locate and extinguish a fire. The agent can either tackle the fire alone or collaborate with other agents to expedite the process. Collaboration is key, as the collective effort of multiple agents significantly increases efficiency.

## Training Dataset
![Incremental Complexity (2-5 rooms)](https://github.com/duyhho/AI-Empowered-Robotics/blob/main/Media/2-5%20Rooms.png)
![Out-of-distribution Complexity](https://github.com/duyhho/AI-Empowered-Robotics/blob/main/Media/6-10%20Rooms.png)
### Training Settings:
The agent starts in the first room, while the fire's location is randomized in another. Other agents are placed throughout the building but are initially inactive. They become active once the primary agent makes contact, thereafter following the primary agent to the fire. The situation is resolved when the agents collectively reach the fire.

### Approach:
The optimal strategy utilizes a combination of Proximal Policy Optimization (PPO), Curiosity-driven Exploration, and Incentive-based Curriculum Learning.

#### PPO (Proximal Policy Optimization)
PPO is an advanced policy gradient method for reinforcement learning. It's designed to maintain a balance between exploration and exploitation by using a clipped objective to prevent large policy updates, which ensures more stable and consistent learning.

#### Curiosity-Driven Exploration
Curiosity within the context of reinforcement learning refers to an intrinsic reward mechanism that encourages the agent to explore previously unseen states. This is achieved by measuring the difference between the predicted and actual outcomes of an agent's actions, with larger differences yielding higher intrinsic rewards.

#### Curriculum Learning
Curriculum Learning in AI is akin to structured education for humans—it involves starting training with simpler tasks and progressively moving to more complex ones. The timeline-based curriculum changes the environment's complexity according to a predefined schedule, while incentive-based (reward-based) curriculum learning adapts the complexity dynamically based on the agent's performance, providing rewards as incentives for completing more complex tasks successfully.

We have designed a curriculum that scales the complexity of room settings, starting from a basic 2-room configuration. The agent progresses to higher levels (3-, 4-, and 5-room settings) once a proficiency threshold is achieved, ensuring foundational skills are solidified before introducing additional complexity.


### Metrics (Training)

#### Training Metrics:
The agent undergoes training in randomized 2- to 5-room settings for 5 million steps. Rewards are assigned upon reaching the fire: +2 for extinguishing the fire, and an additional +2 for each activated agent. A slight negative reward is issued throughout the navigation to incentivize quick and efficient task completion.

![Training Metrics Graph](https://github.com/duyhho/AI-Empowered-Robotics/blob/main/Media/Training.PNG)

#### Evaluation:
The evaluation phase involves testing the agent's learned behaviors in scenarios similar to the training settings to measure performance and robustness.

![Evaluation](https://github.com/duyhho/AI-Empowered-Robotics/blob/main/Media/Evaluation.PNG)

#### OOD Settings (Out-of-Distribution):
Out-of-Distribution settings test the agents in 6- to 10-room configurations, which are significantly more complex than the training environments. This assesses the agent's ability to generalize and adapt to new challenges.

![Evaluation-OOD](https://github.com/duyhho/AI-Empowered-Robotics/blob/main/Media/Evaluation-OOD.PNG) 

### Conclusion:
The comparative analysis of curriculum designs demonstrates that the incentive-based curriculum design excels not only in training but also in both in-distribution and out-of-distribution evaluations. Whether the environment is static or dynamic, the curriculum that transitions from static rooms to dynamic rooms yields superior performance compared to those utilizing only static or dynamic settings. This pattern holds true even when the agent is subjected to OOD settings, which are inherently more complex and unpredictable. The incentive-based approach, which adapts to the agent's progress by introducing more complex tasks when certain performance thresholds are met, proves to be more effective than a timeline-driven curriculum across all tested environments. This adaptability is crucial for preparing autonomous systems to handle real-world variables, especially in high-stakes applications like collaborative firefighting, where efficiency and rapid adaptability can be lifesaving.

### Supplementary Materials 

#### Demo Video

<video src = "https://github.com/duyhho/AI-Empowered-Robotics/blob/main/Media/Fire%20Navigation%20-%20Copy.mp4"></video>


