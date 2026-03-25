'use client';

import { useState, useEffect } from 'react';
import { Failure, UpdateFailureDto, createJiraTicket, retryFailure, updateFailure } from '../lib/api';
import { Loader2, Ticket, RotateCw, Save, X, ExternalLink, AlertCircle, CheckCircle, Clock } from 'lucide-react';

interface FailureDetailProps {
  failure: Failure;
  onClose: () => void;
  onUpdate: (updatedFailure: Failure) => void;
}

export default function FailureDetail({ failure, onClose, onUpdate }: FailureDetailProps) {
  const [formData, setFormData] = useState<UpdateFailureDto>({
    classification: failure.classification || '',
    confidence: failure.confidence,
    summary: failure.summary || '',
    rootCause: failure.rootCause || '',
    suggestedFix: failure.suggestedFix || '',
    status: failure.status,
  });

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    setFormData({
      classification: failure.classification || '',
      confidence: failure.confidence,
      summary: failure.summary || '',
      rootCause: failure.rootCause || '',
      suggestedFix: failure.suggestedFix || '',
      status: failure.status,
    });
  }, [failure]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
  };

  const handleSave = async () => {
    setLoading(true);
    setError(null);
    try {
      const updated = await updateFailure(failure.id, formData);
      onUpdate(updated);
    } catch (err) {
      setError('Failed to update failure');
    } finally {
      setLoading(false);
    }
  };

  const handleCreateJira = async () => {
    setLoading(true);
    setError(null);
    try {
      const updated = await createJiraTicket(failure.id);
      onUpdate(updated);
    } catch (err) {
      setError('Failed to create Jira ticket');
    } finally {
      setLoading(false);
    }
  };

  const handleRetry = async () => {
    setLoading(true);
    setError(null);
    try {
      const updated = await retryFailure(failure.id);
      onUpdate(updated);
    } catch (err) {
      setError('Failed to retry failure');
    } finally {
      setLoading(false);
    }
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Resolved': return 'bg-green-100 text-green-800 border-green-200';
      case 'NeedsIntervention': return 'bg-red-100 text-red-800 border-red-200';
      default: return 'bg-blue-100 text-blue-800 border-blue-200';
    }
  };

  const getStatusIcon = (status: string) => {
    switch (status) {
      case 'Resolved': return <CheckCircle className="w-4 h-4 mr-1" />;
      case 'NeedsIntervention': return <AlertCircle className="w-4 h-4 mr-1" />;
      default: return <Clock className="w-4 h-4 mr-1" />;
    }
  };

  return (
    <div className="fixed inset-y-0 right-0 w-full sm:w-2/3 lg:w-1/2 bg-white shadow-2xl border-l border-gray-200 transform transition-transform overflow-y-auto z-50 flex flex-col">
      {/* Header */}
      <div className="flex items-center justify-between p-6 border-b border-gray-200 bg-gray-50 sticky top-0 z-10">
        <div>
           <h2 className="text-xl font-bold text-gray-900">Failure Details</h2>
           <p className="text-sm text-gray-500 mt-1">ID: {failure.id}</p>
        </div>
        <button
          onClick={onClose}
          className="p-2 rounded-full text-gray-400 hover:text-gray-600 hover:bg-gray-100 transition-colors focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
          aria-label="Close"
        >
          <X className="w-6 h-6" />
        </button>
      </div>

      <div className="flex-1 p-6 space-y-8">
        {error && (
          <div className="flex items-center p-4 text-sm text-red-800 border border-red-200 rounded-lg bg-red-50" role="alert">
            <AlertCircle className="flex-shrink-0 inline w-5 h-5 mr-3" />
            <span className="sr-only">Error:</span>
            <div>{error}</div>
          </div>
        )}

        {/* Primary Actions */}
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <Button
            onClick={handleCreateJira}
            disabled={loading || failure.jiraCreated}
            variant={failure.jiraCreated ? "outline" : "primary"}
            className="w-full justify-center"
          >
            {loading ? <Loader2 className="animate-spin mr-2 h-4 w-4" /> : <Ticket className="mr-2 h-4 w-4" />}
            {failure.jiraCreated ? 'Jira Ticket Created' : 'Create Jira Ticket'}
          </Button>

          <Button
            onClick={handleRetry}
            disabled={loading || failure.retryAttempted}
            variant={failure.retryAttempted ? "outline" : "secondary"}
            className="w-full justify-center"
          >
            {loading ? <Loader2 className="animate-spin mr-2 h-4 w-4" /> : <RotateCw className="mr-2 h-4 w-4" />}
            {failure.retryAttempted ? 'Retry Initiated' : 'Retry Pipeline'}
          </Button>
        </div>

        {/* Jira Link if exists */}
        {failure.jiraTicketId && failure.jiraTicketUrl && (
            <div className="bg-blue-50 border border-blue-200 rounded-md p-4 flex items-center justify-between">
                <div className="flex items-center">
                    <Ticket className="h-5 w-5 text-blue-600 mr-2" />
                    <span className="font-medium text-blue-900">Jira Ticket: {failure.jiraTicketId}</span>
                </div>
                <a 
                    href={failure.jiraTicketUrl} 
                    target="_blank" 
                    rel="noopener noreferrer"
                    className="flex items-center text-sm font-medium text-blue-700 hover:text-blue-900 hover:underline"
                >
                    View in Jira <ExternalLink className="h-3 w-3 ml-1" />
                </a>
            </div>
        )}

        {/* Read-only Information Section */}
        <section aria-labelledby="failure-info-heading" className="bg-gray-50 rounded-lg border border-gray-200 p-5">
            <h3 id="failure-info-heading" className="text-sm font-semibold text-gray-900 uppercase tracking-wider mb-4 border-b border-gray-200 pb-2">
                Pipeline Information
            </h3>
            <div className="grid grid-cols-1 gap-y-4 sm:grid-cols-2 sm:gap-x-6 sm:gap-y-6">
                <div>
                    <dt className="text-xs font-medium text-gray-500 uppercase">Pipeline Name</dt>
                    <dd className="mt-1 text-sm font-medium text-gray-900 break-words">{failure.pipelineName}</dd>
                </div>
                 <div>
                    <dt className="text-xs font-medium text-gray-500 uppercase">Run ID</dt>
                    <dd className="mt-1 text-sm font-mono text-gray-700 bg-white px-2 py-1 rounded border border-gray-200 w-fit">{failure.runId}</dd>
                </div>
                <div className="sm:col-span-2">
                    <dt className="text-xs font-medium text-gray-500 uppercase mb-2">Error Message</dt>
                    <dd className="text-sm font-mono text-red-700 bg-red-50 border border-red-100 rounded-md p-3 overflow-x-auto whitespace-pre-wrap leading-relaxed shadow-sm">
                        {failure.errorMessage}
                    </dd>
                </div>
            </div>
        </section>

        {/* Mutable Analysis Section */}
        <section aria-labelledby="analysis-heading" className="space-y-6 pt-4">
            <div className="flex items-center justify-between mb-4">
                <h3 id="analysis-heading" className="text-lg font-medium leading-6 text-gray-900">
                    Triage Analysis
                </h3>
                <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium border ${getStatusColor(formData.status || 'Open')}`}>
                    {getStatusIcon(formData.status || 'Open')}
                    Current Status: {failure.status}
                </span>
            </div>
          
            <div className="bg-white rounded-lg space-y-5">
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-5">
                    <div>
                        <label htmlFor="status" className="block text-sm font-semibold text-gray-700 mb-1">Status</label>
                        <select
                            id="status"
                            name="status"
                            value={formData.status}
                            onChange={handleChange}
                            className="text-gray-900 mt-1 block w-full pl-3 pr-10 py-2 text-base border-gray-300 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm rounded-md shadow-sm border transaction-colors hover:border-gray-400"
                        >
                            <option value="Open">Open</option>
                            <option value="Resolved">Resolved</option>
                            <option value="NeedsIntervention">Needs Intervention</option>
                        </select>
                    </div>

                    <div>
                         <label htmlFor="classification" className="block text-sm font-semibold text-gray-700 mb-1">Classification</label>
                         <input
                            type="text"
                            id="classification"
                            name="classification"
                            value={formData.classification}
                            onChange={handleChange}
                            className="text-gray-900 mt-1 block w-full border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 sm:text-sm p-2 border hover:border-gray-400"
                            placeholder="e.g. Infrastructure, Code, Test"
                        />
                    </div>
                </div>
                
                 <div>
                    <label htmlFor="confidence" className="block text-sm font-semibold text-gray-700 mb-1">
                        Confidence Score (0.0 - 1.0)
                    </label>
                    <input
                        type="number"
                        id="confidence"
                        name="confidence"
                        value={formData.confidence}
                        onChange={handleChange}
                        min="0"
                        max="1"
                        step="0.1"
                        className="text-gray-900 mt-1 block w-full sm:w-1/3 border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 sm:text-sm p-2 border hover:border-gray-400"
                    />
                </div>

                <div>
                    <label htmlFor="summary" className="block text-sm font-semibold text-gray-700 mb-1">Summary</label>
                    <textarea
                        id="summary"
                        name="summary"
                        rows={3}
                        value={formData.summary}
                        onChange={handleChange}
                        className="text-gray-900 mt-1 block w-full border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 sm:text-sm p-2 border hover:border-gray-400"
                        placeholder="Brief summary of the failure..."
                    />
                </div>

                <div>
                    <label htmlFor="rootCause" className="block text-sm font-semibold text-gray-700 mb-1">Root Cause</label>
                    <textarea
                        id="rootCause"
                        name="rootCause"
                        rows={3}
                        value={formData.rootCause}
                        onChange={handleChange}
                        className="text-gray-900 mt-1 block w-full border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 sm:text-sm p-2 border hover:border-gray-400"
                        placeholder="Detailed root cause analysis..."
                    />
                </div>
                
                <div>
                    <label htmlFor="suggestedFix" className="block text-sm font-semibold text-gray-700 mb-1">Suggested Fix</label>
                    <textarea
                        id="suggestedFix"
                        name="suggestedFix"
                        rows={3}
                        value={formData.suggestedFix}
                        onChange={handleChange}
                        className="text-gray-900 mt-1 block w-full border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 sm:text-sm p-2 border hover:border-gray-400"
                        placeholder="Proposed solution or fix..."
                    />
                </div>
            </div>
        </section>
      </div>

      <div className="border-t border-gray-200 px-6 py-4 bg-gray-50 flex justify-end gap-3 sticky bottom-0 z-10">
        <Button onClick={onClose} variant="outline" disabled={loading}>
            Cancel
        </Button>
        <Button 
            onClick={handleSave} 
            disabled={loading} 
            variant="primary" 
            className="min-w-[100px]"
        >
            {loading ? <Loader2 className="animate-spin mr-2 h-4 w-4" /> : <Save className="mr-2 h-4 w-4" />}
            Save Changes
        </Button>
      </div>
    </div>
  );
}

// Minimal Button component to avoid extra dependency
interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'primary' | 'secondary' | 'outline' | 'default';
  className?: string;
  children: React.ReactNode;
}

function Button({ variant = 'default', className = '', ...props }: ButtonProps) {
  const baseStyles = "inline-flex items-center px-4 py-2 border text-sm font-medium rounded-md focus:outline-none focus:ring-2 focus:ring-offset-2 transition-colors shadow-sm";
  
  let variantStyles = "";
  switch (variant) {
    case 'primary':
    case 'default':
      variantStyles = "border-transparent text-white bg-blue-600 hover:bg-blue-700 focus:ring-blue-500 disabled:bg-blue-300";
      break;
    case 'secondary':
      variantStyles = "border-transparent text-blue-700 bg-blue-100 hover:bg-blue-200 focus:ring-blue-500 disabled:bg-blue-50 disabled:text-blue-300";
      break;
    case 'outline':
      variantStyles = "border-gray-300 text-gray-700 bg-white hover:bg-gray-50 focus:ring-blue-500 disabled:bg-gray-50 disabled:text-gray-400";
      break;
  }

  return (
    <button
      type="button"
      className={`${baseStyles} ${variantStyles} ${className}`}
      {...props}
    />
  );
}
