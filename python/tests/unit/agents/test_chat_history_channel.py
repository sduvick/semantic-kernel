# Copyright (c) Microsoft. All rights reserved.

from collections.abc import AsyncIterable

import pytest

from semantic_kernel.agents.chat_history_channel import ChatHistoryAgentProtocol, ChatHistoryChannel
from semantic_kernel.contents.chat_message_content import ChatMessageContent
from semantic_kernel.contents.streaming_chat_message_content import StreamingChatMessageContent
from semantic_kernel.contents.utils.author_role import AuthorRole
from semantic_kernel.exceptions import ServiceInvalidTypeError


class MockChatHistoryHandler:
    """Mock agent to test chat history handling"""

    async def invoke(self, history: list[ChatMessageContent]) -> AsyncIterable[ChatMessageContent]:
        for message in history:
            yield ChatMessageContent(role=AuthorRole.SYSTEM, content=f"Processed: {message.content}")

    async def invoke_stream(self, history: list[ChatMessageContent]) -> AsyncIterable["StreamingChatMessageContent"]:
        pass


class MockNonChatHistoryHandler:
    """Mock agent to test incorrect instance handling."""

    id: str = "mock_non_chat_history_handler"


ChatHistoryAgentProtocol.register(MockChatHistoryHandler)


@pytest.mark.asyncio
async def test_invoke():
    channel = ChatHistoryChannel()
    agent = MockChatHistoryHandler()

    initial_message = ChatMessageContent(role=AuthorRole.USER, content="Initial message")
    channel.messages.append(initial_message)

    received_messages = []
    async for message in channel.invoke(agent):
        received_messages.append(message)
        break  # only process one message for the test

    assert len(received_messages) == 1
    assert "Processed: Initial message" in received_messages[0].content


@pytest.mark.asyncio
async def test_invoke_incorrect_instance_throws():
    channel = ChatHistoryChannel()
    agent = MockNonChatHistoryHandler()

    with pytest.raises(ServiceInvalidTypeError):
        async for _ in channel.invoke(agent):
            pass


@pytest.mark.asyncio
async def test_receive():
    channel = ChatHistoryChannel()
    history = [
        ChatMessageContent(role=AuthorRole.SYSTEM, content="test message 1"),
        ChatMessageContent(role=AuthorRole.USER, content="test message 2"),
    ]

    await channel.receive(history)

    assert len(channel.messages) == 2
    assert channel.messages[0].content == "test message 1"
    assert channel.messages[0].role == AuthorRole.SYSTEM
    assert channel.messages[1].content == "test message 2"
    assert channel.messages[1].role == AuthorRole.USER


@pytest.mark.asyncio
async def test_get_history():
    channel = ChatHistoryChannel()
    history = [
        ChatMessageContent(role=AuthorRole.SYSTEM, content="test message 1"),
        ChatMessageContent(role=AuthorRole.USER, content="test message 2"),
    ]
    channel.messages.extend(history)

    messages = [message async for message in channel.get_history()]

    assert len(messages) == 2
    assert messages[0].content == "test message 2"
    assert messages[0].role == AuthorRole.USER
    assert messages[1].content == "test message 1"
    assert messages[1].role == AuthorRole.SYSTEM
