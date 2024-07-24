# Copyright (c) Microsoft. All rights reserved.

import uuid
from abc import ABC
from typing import ClassVar

from pydantic import Field

from semantic_kernel.agents.agent_channel import AgentChannel
from semantic_kernel.kernel import Kernel
from semantic_kernel.kernel_pydantic import KernelBaseModel
from semantic_kernel.utils.experimental_decorator import experimental_class


@experimental_class
class Agent(ABC, KernelBaseModel):
    """Base abstraction for all Semantic Kernel agents.

    An agent instance may participate in one or more conversations.
    A conversation may include one or more agents.
    In addition to identity and descriptive meta-data, an Agent
    must define its communication protocol, or AgentChannel.

    Attributes:
        name: The name of the agent (optional).
        description: The description of the agent (optional).
        id: The unique identifier of the agent (optional). If no id is provided,
            a new UUID will be generated.
        instructions: The instructions for the agent (optional
    """

    id: str = Field(default_factory=lambda: str(uuid.uuid4()))
    description: str | None = None
    name: str | None = None
    instructions: str | None = None
    kernel: Kernel = Field(default_factory=Kernel)
    channel_type: ClassVar[type[AgentChannel] | None] = None

    def get_channel_keys(self) -> list[str]:
        """Get the channel keys.

        Returns:
            A list of channel keys.
        """
        if not self.channel_type:
            raise NotImplementedError("Unable to get channel keys. Channel type not configured.")
        return [self.channel_type.__name__]

    def create_channel(self) -> AgentChannel:
        """Create a channel.

        Returns:
            An instance of AgentChannel.
        """
        if not self.channel_type:
            raise NotImplementedError("Unable to create channel. Channel type not configured.")
        return self.channel_type()
