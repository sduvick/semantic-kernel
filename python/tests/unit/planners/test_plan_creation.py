# Copyright (c) Microsoft. All rights reserved.

from semantic_kernel.core_plugins.math_plugin import MathPlugin
from semantic_kernel.functions.kernel_arguments import KernelArguments
from semantic_kernel.kernel import Kernel
from semantic_kernel.planners import Plan


def test_create_empty_plan():
    plan = Plan()
    assert plan is not None
    assert plan.name.startswith("plan_")
    assert type(plan.state) is KernelArguments
    assert plan.plugin_name.startswith("p_")
    assert plan.description == ""
    assert plan.function is None
    assert type(plan.parameters) is KernelArguments
    assert plan.is_prompt is None
    assert plan.is_native is None
    assert plan.prompt_execution_settings is None
    assert plan.has_next_step is False
    assert plan.next_step_index == 0
    assert plan._steps == []


def test_create_plan_with_name():
    plan = Plan(name="test")
    assert plan is not None
    assert plan.name == "test"
    assert type(plan.state) is KernelArguments
    assert plan.plugin_name.startswith("p_")
    assert plan.description == ""
    assert plan.function is None
    assert type(plan.parameters) is KernelArguments
    assert plan.is_prompt is None
    assert plan.is_native is None
    assert plan.prompt_execution_settings is None
    assert plan.has_next_step is False
    assert plan.next_step_index == 0
    assert plan._steps == []


def test_create_plan_with_name_and_description():
    plan = Plan(name="test", description="test description")
    assert plan is not None
    assert plan.name == "test"
    assert type(plan.state) is KernelArguments
    assert plan.plugin_name.startswith("p_")
    assert plan.description == "test description"
    assert plan.function is None
    assert type(plan.parameters) is KernelArguments
    assert plan.is_prompt is None
    assert plan.is_native is None
    assert plan.prompt_execution_settings is None
    assert plan.has_next_step is False
    assert plan.next_step_index == 0
    assert plan._steps == []


def test_create_plan_with_state_and_parameters():
    plan = Plan(name="test", state=KernelArguments(), parameters=KernelArguments(**{"test_param": "test_param_val"}))
    assert plan is not None
    assert plan.name == "test"
    assert plan.state.get("input", "") == ""
    assert plan.plugin_name.startswith("p_")
    assert plan.description == ""
    assert plan.function is None
    assert plan.parameters["test_param"] == "test_param_val"
    assert plan.is_prompt is None
    assert plan.is_native is None
    assert plan.prompt_execution_settings is None
    assert plan.has_next_step is False
    assert plan.next_step_index == 0
    assert plan._steps == []


def test_create_plan_with_name_and_native_function(kernel: Kernel):
    # import test (math) plugin
    plugin = MathPlugin()
    kernel.add_plugin(plugin, "math")

    test_function = kernel.get_function("math", "Add")

    plan = Plan(name="test", function=test_function)
    assert plan is not None
    assert plan.name == "Add"
    assert type(plan.state) is KernelArguments
    assert plan.plugin_name == "math"
    assert plan.description == test_function.description
    assert plan.function is test_function
    assert type(plan.parameters) is KernelArguments
    assert plan.is_prompt is test_function.is_prompt
    assert plan.is_native is not test_function.is_prompt
    assert plan.prompt_execution_settings is None
    assert plan.has_next_step is False
    assert plan.next_step_index == 0
    assert plan._steps == []


def test_create_multistep_plan_with_functions(kernel: Kernel):
    # import test (math) plugin
    kernel.add_plugin(MathPlugin(), "math")

    test_function1 = kernel.get_function("math", "Add")
    test_function2 = kernel.get_function("math", "Subtract")

    plan = Plan(name="multistep_test")
    plan.add_steps([test_function1, test_function2])

    assert plan is not None
    assert plan.name == "multistep_test"
    assert type(plan.state) is KernelArguments
    assert plan.plugin_name.startswith("p_")
    assert plan.description == ""
    assert plan.function is None
    assert type(plan.parameters) is KernelArguments
    assert plan.is_prompt is None
    assert plan.is_native is None
    assert plan.prompt_execution_settings is None
    assert plan.has_next_step is True
    assert plan.next_step_index == 0
    assert len(plan._steps) == 2


def test_create_multistep_plan_with_plans(kernel: Kernel):
    kernel.add_plugin(MathPlugin(), "math")

    test_function1 = kernel.get_function("math", "Add")
    test_function2 = kernel.get_function("math", "Subtract")

    plan = Plan(name="multistep_test")
    plan_step1 = Plan(name="step1", function=test_function1)
    plan_step2 = Plan(name="step2", function=test_function2)
    plan.add_steps([plan_step1, plan_step2])

    assert plan is not None
    assert plan.name == "multistep_test"
    assert type(plan.state) is KernelArguments
    assert plan.plugin_name.startswith("p_")
    assert plan.description == ""
    assert plan.function is None
    assert type(plan.parameters) is KernelArguments
    assert plan.is_prompt is None
    assert plan.is_native is None
    assert plan.prompt_execution_settings is None
    assert plan.has_next_step is True
    assert plan.next_step_index == 0
    assert len(plan._steps) == 2


def test_add_step_to_plan(kernel: Kernel):
    kernel.add_plugin(MathPlugin(), "math")

    test_function1 = kernel.get_function("math", "Add")
    test_function2 = kernel.get_function("math", "Subtract")

    plan = Plan(name="multistep_test", function=test_function1)
    plan.add_steps([test_function2])
    assert plan is not None
    assert plan.name == "Add"
    assert type(plan.state) is KernelArguments
    assert plan.plugin_name == "math"
    assert plan.description == test_function1.description
    assert plan.function is test_function1
    assert type(plan.parameters) is KernelArguments
    assert plan.is_prompt is test_function1.is_prompt
    assert plan.is_native is not test_function1.is_prompt
    assert plan.prompt_execution_settings is None
    assert plan.has_next_step is True
    assert plan.next_step_index == 0
    assert len(plan._steps) == 1
