import mlagents
from mlagents_envs.environment import UnityEnvironment

from dqn_agent import Agent

#-------------------------------------------------------------------------

# Create environment
env_name = 'builds/CarAgent5_single'
env = UnityEnvironment(file_name=env_name, seed=1, side_channels=[])
env.reset()

# Get behavior
behavior_name = list(env.behavior_specs)[0]
print(f"Name of the behavior : {behavior_name}")

# Get behavior details - observations
spec = env.behavior_specs[behavior_name]
obs_spec = spec.observation_specs
obs_groups = len(obs_spec)
print("Amount of input channels : ", obs_groups)
print("obs_spec : ", obs_spec)

observation_shape = []
for i in range(obs_groups):
    obs_shape = obs_spec[i][0]
    observation_shape.append(obs_shape)
    print(f"Shape of input channel {i} : {obs_shape}")


observation_count = len(observation_shape)
print("Number of observations : ", observation_count)
print("Observation vector shape: ", observation_shape)

# Get behavior details - actions
action_spec = spec.action_spec
if action_spec.continuous_size > 0:
  print(f"The behavior uses continuous actions= {action_spec.continuous_size}")
if action_spec.discrete_size > 0:
  print(f"The behavior uses discrete actions= {action_spec.discrete_branches}")
  

# Get initial entironment steps
decision_steps, terminal_steps = env.get_steps(behavior_name)
print(decision_steps.obs)

#-------------------------------------------------------------------------

# Model parameters
STATE_SPACE = obs_shape
ACTION_SPACE = action_spec.discrete_branches
REPLAY_CAPACITY = 10240
BATCH_SIZE = 32
UPDATE_INTERVAL = 15000

DISCOUNT = 0.95
TAU = 1

# exploration
EPSILON = 1
EPSILON_DECAY = 0.999
MIN_EPSILON = 0.01

# learning rate
ALPHA = 0.003
ALPHA_DECAY = 1
MIN_ALPHA = 0.001

NR_EPISODES = 10


config = {
    'epsilon':EPSILON,
    'epsilon_decay':EPSILON_DECAY,
    'min_epsilon':MIN_EPSILON,

    'alpha':ALPHA,
    'discount':DISCOUNT,
    'tau':TAU,
}

env.reset()
agent = Agent(state_size=STATE_SPACE, action_size=ACTION_SPACE, dqn_type='DQN')

#-------------------------------------------------------------------------

for episode in range(0, NR_EPISODES):
  env.reset()
  decision_steps, terminal_steps = env.get_steps(behavior_name)
  tracked_agent = -1 # -1 indicates not yet tracking

  # For the tracked_agent
  done = False
  episode_rewards = 0 

  while not done:
    # Track the first agent we see if not tracking
    # Note : len(decision_steps) = [number of agents that requested a decision]
    if tracked_agent == -1 and len(decision_steps) >= 1:
      tracked_agent = decision_steps.agent_id[0]

    state = env_info.vector_observations[0]

    # Generate an action for all agents
    action = agent.act(state, epsilon)

    # Set the actions + Move the simulation forward
    env.set_actions(behavior_name, action)
    env.step()

    # Get the new simulation results
    decision_steps, terminal_steps = env.get_steps(behavior_name)

    if tracked_agent in decision_steps: # The agent requested a decision
      episode_rewards += decision_steps[tracked_agent].reward
    if tracked_agent in terminal_steps: # The agent terminated its episode
      episode_rewards += terminal_steps[tracked_agent].reward
      done = True

  print(f"Total rewards for episode {episode} is {episode_rewards}")



env.close()
print("Closed environment")