using TaskQueueing.ObjectModel.Enums;

namespace TaskQueueing.Test;

public record class TestMessage : AbstractMessage { }

public class MessagesTest
{
    [Fact]
    public void SetMessageStateProcessedTrueTest()
    {
        var message = new TestMessage();
        Assert.Equal(MessageState.Undefined, message.State);

        message.Processed = true;
        Assert.Equal(MessageState.Processed, message.State);
        Assert.True(message.Processed);
    }

    [Fact]
    public void SetMessageStateNotProcessedTest()
    {
        var message = new TestMessage()
        {
            State = MessageState.Processed
        };
        Assert.Equal(MessageState.Processed, message.State);

        message.Processed = false;
        Assert.Equal(MessageState.Undefined, message.State);
        Assert.False(message.Processed);
    }

    [Theory]
    [MemberData(nameof(ProcessedStateTestCases))]
    public void SetMessageStateProcessedOtherStateUnchangedTest(
        MessageState baseState, 
        MessageState lifecycleState, 
        Func<TestMessage, bool> stateCheck
    )
    {
        var initialState = baseState | lifecycleState;
        var message = new TestMessage()
        {
            State = initialState
        };
        Assert.Equal(initialState, message.State);
        Assert.True(stateCheck(message));

        // Lifecycle state should be unchanged
        message.Processed = false;
        Assert.Equal(initialState, message.State);
        Assert.True(stateCheck(message));

        // Lifecycle state should be exactly Processed
        message.Processed = true;
        Assert.Equal(baseState | MessageState.Processed, message.State);
        Assert.True(stateCheck(message));
        Assert.True(message.Processed);
        if (lifecycleState != MessageState.Undefined)
        {
            // If it is undefined (zero) it will always be equal
            Assert.NotEqual(lifecycleState, message.State & lifecycleState);
        }

        // Lifecycle state should be exactly base state
        message.Processed = false;
        Assert.Equal(baseState, message.State);
        Assert.True(stateCheck(message));
        Assert.False(message.Processed);
    }

    [Fact]
    public void SetMessageStateProcessingTest()
    {
        var message = new TestMessage();
        Assert.Equal(MessageState.Undefined, message.State);

        message.Processing = true;
        Assert.Equal(MessageState.Processing, message.State);
        Assert.True(message.Processing);
    }

    [Fact]
    public void SetMessageStateNotProcessingTest()
    {
        var message = new TestMessage()
        {
            State = MessageState.Processing
        };
        Assert.Equal(MessageState.Processing, message.State);

        message.Processing = false;
        Assert.Equal(MessageState.Undefined, message.State);
        Assert.False(message.Processing);
    }

    [Theory]
    [MemberData(nameof(ProcessingStateTestCases))]
    public void SetMessageStateProcessingOtherStateUnchangedTest(
        MessageState baseState, 
        MessageState lifecycleState, 
        Func<TestMessage, bool> stateCheck
    )
    {
        var initialState = baseState | lifecycleState;
        var message = new TestMessage()
        {
            State = initialState
        };
        Assert.Equal(initialState, message.State);
        Assert.True(stateCheck(message));

        // Lifecycle state should be unchanged
        message.Processing = false;
        Assert.Equal(initialState, message.State);
        Assert.True(stateCheck(message));

        // Lifecycle state should be exactly Processing
        message.Processing = true;
        Assert.Equal(baseState | MessageState.Processing, message.State);
        Assert.True(stateCheck(message));
        Assert.True(message.Processing);
        if (lifecycleState != MessageState.Undefined)
        {
            // If it is undefined (zero) it will always be equal
            Assert.NotEqual(lifecycleState, message.State & lifecycleState);
        }

        // Lifecycle state should be exactly base state
        message.Processing = false;
        Assert.Equal(baseState, message.State);
        Assert.True(stateCheck(message));
        Assert.False(message.Processing);
    }
    [Fact]
    public void SetMessageStateFailedTest()
    {
        var message = new TestMessage();
        Assert.Equal(MessageState.Undefined, message.State);

        message.Failed = true;
        Assert.Equal(MessageState.Error, message.State);
        Assert.True(message.Failed);
    }

    [Fact]
    public void SetMessageStateNotFailedTest()
    {
        var message = new TestMessage()
        {
            State = MessageState.Error
        };
        Assert.Equal(MessageState.Error, message.State);

        message.Failed = false;
        Assert.Equal(MessageState.Undefined, message.State);
        Assert.False(message.Failed);
    }

    [Theory]
    [MemberData(nameof(ErrorStateTestCases))]
    public void SetMessageStateFailedOtherStateUnchangedTest(
        MessageState baseState, 
        MessageState lifecycleState, 
        Func<TestMessage, bool> stateCheck
    )
    {
        var initialState = baseState | lifecycleState;
        var message = new TestMessage()
        {
            State = initialState
        };
        Assert.Equal(initialState, message.State);
        Assert.True(stateCheck(message));

        // Lifecycle state should be unchanged
        message.Failed = false;
        Assert.Equal(initialState, message.State);
        Assert.True(stateCheck(message));

        // Lifecycle state should be exactly Error
        message.Failed = true;
        Assert.Equal(baseState | MessageState.Error, message.State);
        Assert.True(stateCheck(message));
        Assert.True(message.Failed);
        if (lifecycleState != MessageState.Undefined)
        {
            // If it is undefined (zero) it will always be equal
            Assert.NotEqual(lifecycleState, message.State & lifecycleState);
        }

        // Lifecycle state should be exactly base state
        message.Failed = false;
        Assert.Equal(baseState, message.State);
        Assert.True(stateCheck(message));
        Assert.False(message.Failed);
    }

    public static IEnumerable<object[]> MessageStateTestCases()
    {
        yield return new object[] { MessageState.Posted,   MessageState.Undefined, (Func<TestMessage, bool>)((m) => m.Posted) };
        yield return new object[] { MessageState.Received, MessageState.Undefined, (Func<TestMessage, bool>)((m) => m.Received) };
        yield return new object[] { MessageState.Posted,   MessageState.Processing, (Func<TestMessage, bool>)((m) => m.Posted) };
        yield return new object[] { MessageState.Received, MessageState.Processing, (Func<TestMessage, bool>)((m) => m.Received) };
        yield return new object[] { MessageState.Posted,   MessageState.Error, (Func<TestMessage, bool>)((m) => m.Posted) };
        yield return new object[] { MessageState.Received, MessageState.Error, (Func<TestMessage, bool>)((m) => m.Received) };
        yield return new object[] { MessageState.Posted,   MessageState.Processed, (Func<TestMessage, bool>)((m) => m.Posted) };
        yield return new object[] { MessageState.Received, MessageState.Processed, (Func<TestMessage, bool>)((m) => m.Received) };
    }

    public static IEnumerable<object[]> ProcessedStateTestCases()
    {
        return MessageStateTestCases().Where((o) => (MessageState)o[1] != MessageState.Processed);
    }

    public static IEnumerable<object[]> ProcessingStateTestCases()
    {
        return MessageStateTestCases().Where((o) => (MessageState)o[1] != MessageState.Processing);
    }

    public static IEnumerable<object[]> ErrorStateTestCases()
    {
        return MessageStateTestCases().Where((o) => (MessageState)o[1] != MessageState.Error);
    }
}