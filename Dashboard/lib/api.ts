
export interface Failure {
    id: string;
    runId: string;
    pipelineName: string;
    activityName?: string;
    errorMessage: string;
    classification?: string;
    confidence: number;
    summary?: string;
    rootCause?: string;
    suggestedFix?: string;
    sourceJson?: string;
    autoHandled: boolean;
    jiraCreated: boolean;
    jiraTicketId?: string;
    jiraTicketUrl?: string;
    retryAttempted: boolean;
    status: 'Open' | 'Resolved' | 'NeedsIntervention';
    createdAt: string;
    updatedAt: string;
}

export interface UpdateFailureDto {
    classification?: string;
    confidence?: number;
    summary?: string;
    rootCause?: string;
    suggestedFix?: string;
    status?: string;
}

const API_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000';

export async function getFailures(): Promise<Failure[]> {
    const response = await fetch(`${API_URL}/failures`, { cache: 'no-store' });
    if (!response.ok) {
        throw new Error('Failed to fetch failures');
    }
    return response.json();
}

export async function getFailureById(id: string): Promise<Failure> {
    const response = await fetch(`${API_URL}/failures/${id}`, { cache: 'no-store' });
    if (!response.ok) {
        throw new Error('Failed to fetch failure');
    }
    return response.json();
}

export async function updateFailure(id: string, updates: UpdateFailureDto): Promise<Failure> {
    const response = await fetch(`${API_URL}/failures/${id}`, {
        method: 'PATCH',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(updates),
    });
    if (!response.ok) {
        throw new Error('Failed to update failure');
    }
    return response.json();
}

export async function createJiraTicket(id: string): Promise<Failure> {
    const response = await fetch(`${API_URL}/failures/${id}/jira`, {
        method: 'POST',
    });
    if (!response.ok) {
        throw new Error('Failed to create Jira ticket');
    }
    return response.json();
}

export async function retryFailure(id: string): Promise<Failure> {
    const response = await fetch(`${API_URL}/failures/${id}/retry`, {
        method: 'POST',
    });
    if (!response.ok) {
        throw new Error('Failed to retry failure');
    }
    return response.json();
}
