# Copyright (c) Microsoft. All rights reserved.

import pytest

import semantic_kernel as sk
import semantic_kernel.connectors.ai.hugging_face as sk_hf
from semantic_kernel.memory.semantic_text_memory import SemanticTextMemory


@pytest.mark.asyncio
async def test_hf_embeddings_with_memories():
    kernel = sk.Kernel()

    model_id = "sentence-transformers/all-MiniLM-L6-v2"

    embedding_gen = sk_hf.HuggingFaceTextEmbedding(service_id=model_id, ai_model_id=model_id)

    # Configure LLM service
    kernel.add_service(embedding_gen)

    memory = SemanticTextMemory(storage=sk.memory.VolatileMemoryStore(), embeddings_generator=embedding_gen)
    kernel.add_plugin(sk.core_plugins.TextMemoryPlugin(memory), "TextMemoryPlugin")

    await memory.save_reference(
        "test",
        external_id="info1",
        text="this is a test",
        external_source_name="external source",
    )

    # Add some documents to the semantic memory
    await memory.save_information("test", id="info1", text="Sharks are fish.")
    await memory.save_information("test", id="info2", text="Whales are mammals.")
    await memory.save_information("test", id="info3", text="Penguins are birds.")
    await memory.save_information("test", id="info4", text="Dolphins are mammals.")
    await memory.save_information("test", id="info5", text="Flies are insects.")

    # Search for documents
    query = "What are mammals?"
    result = await memory.search("test", query, limit=2, min_relevance_score=0.0)
    print(f"Query: {query}")
    print(f"\tAnswer 1: {result[0].text}")
    print(f"\tAnswer 2: {result[1].text}\n")
    assert "mammals." in result[0].text
    assert "mammals." in result[1].text

    query = "What are fish?"
    result = await memory.search("test", query, limit=1, min_relevance_score=0.0)
    print(f"Query: {query}")
    print(f"\tAnswer: {result[0].text}\n")
    assert result[0].text == "Sharks are fish."

    query = "What are insects?"
    result = await memory.search("test", query, limit=1, min_relevance_score=0.0)
    print(f"Query: {query}")
    print(f"\tAnswer: {result[0].text}\n")
    assert result[0].text == "Flies are insects."

    query = "What are birds?"
    result = await memory.search("test", query, limit=1, min_relevance_score=0.0)
    print(f"Query: {query}")
    print(f"\tAnswer: {result[0].text}\n")
    assert result[0].text == "Penguins are birds."
