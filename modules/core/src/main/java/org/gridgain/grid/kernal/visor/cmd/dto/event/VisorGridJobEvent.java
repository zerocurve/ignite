/* @java.file.header */

/*  _________        _____ __________________        _____
 *  __  ____/___________(_)______  /__  ____/______ ____(_)_______
 *  _  / __  __  ___/__  / _  __  / _  / __  _  __ `/__  / __  __ \
 *  / /_/ /  _  /    _  /  / /_/ /  / /_/ /  / /_/ / _  /  _  / / /
 *  \____/   /_/     /_/   \_,__/   \____/   \__,_/  /_/   /_/ /_/
 */

package org.gridgain.grid.kernal.visor.cmd.dto.event;

import org.gridgain.grid.*;
import org.gridgain.grid.events.*;
import org.gridgain.grid.util.typedef.internal.*;

import java.util.*;

/**
 * Lightweight counterpart for {@link GridJobEvent} .
 */
public class VisorGridJobEvent extends VisorGridEvent {
    /** */
    private static final long serialVersionUID = 0L;

    /** Name of the task that triggered the event. */
    private final String taskName;

    /** Name of task class that triggered the event. */
    private final String taskClassName;

    /** Task session ID of the task that triggered the event. */
    private final GridUuid taskSessionId;

    /** Job ID. */
    private final GridUuid jobId;

    /**
     * Create event with given parameters.
     *
     * @param typeId Event type.
     * @param id Event id.
     * @param name Event name.
     * @param nid Event node ID.
     * @param timestamp Event timestamp.
     * @param message Event message.
     * @param shortDisplay Shortened version of {@code toString()} result.
     * @param taskName Name of the task that triggered the event.
     * @param taskClassName Name of task class that triggered the event.
     * @param taskSessionId Task session ID of the task that triggered the event.
     * @param jobId Job ID.
     */
    public VisorGridJobEvent(
        int typeId,
        GridUuid id,
        String name,
        UUID nid,
        long timestamp,
        String message,
        String shortDisplay,
        String taskName,
        String taskClassName,
        GridUuid taskSessionId,
        GridUuid jobId
    ) {
        super(typeId, id, name, nid, timestamp, message, shortDisplay);

        this.taskName = taskName;
        this.taskClassName = taskClassName;
        this.taskSessionId = taskSessionId;
        this.jobId = jobId;
    }

    /**
     * @return Name of the task that triggered the event.
     */
    public String taskName() {
        return taskName;
    }

    /**
     * @return Name of task class that triggered the event.
     */
    public String taskClassName() {
        return taskClassName;
    }

    /**
     * @return Task session ID of the task that triggered the event.
     */
    public GridUuid taskSessionId() {
        return taskSessionId;
    }

    /**
     * @return Job ID.
     */
    public GridUuid jobId() {
        return jobId;
    }

    /** {@inheritDoc} */
    @Override public String toString() {
        return S.toString(VisorGridJobEvent.class, this);
    }
}
