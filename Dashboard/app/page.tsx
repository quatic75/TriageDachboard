'use client';

import { useState, useEffect } from 'react';
import { Failure, getFailures } from '../lib/api';
import FailureTable from '../components/FailureTable';
import FailureDetail from '../components/FailureDetail';
import { Loader2 } from 'lucide-react';

export default function Home() {
  const [failures, setFailures] = useState<Failure[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [selectedFailure, setSelectedFailure] = useState<Failure | null>(null);

  const fetchFailures = async () => {
    try {
      setLoading(true);
      const data = await getFailures();
      setFailures(data);
      setError(null);
    } catch (err) {
      setError('Failed to load failures. Please try again later.');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchFailures();
  }, []);

  const handleUpdate = (updatedFailure: Failure) => {
    setFailures((prev) =>
      prev.map((f) => (f.id === updatedFailure.id ? updatedFailure : f))
    );
    setSelectedFailure(updatedFailure);
  };

  const closeDetail = () => {
    setSelectedFailure(null);
  };

  return (
    <div className="flex flex-col h-[calc(100vh-8rem)]">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-2xl font-bold text-gray-900">Pipeline Failures</h1>
        <button
          onClick={fetchFailures}
          className="px-4 py-2 bg-white border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
        >
          Refresh
        </button>
      </div>

      {error && (
        <div className="rounded-md bg-red-50 p-4 mb-6">
          <div className="flex">
            <div className="flex-shrink-0">
              <span className="text-red-400">⚠️</span>
            </div>
            <div className="ml-3">
              <h3 className="text-sm font-medium text-red-800">Error loading failures</h3>
              <div className="mt-2 text-sm text-red-700">
                <p>{error}</p>
              </div>
            </div>
          </div>
        </div>
      )}

      {loading && failures.length === 0 ? (
        <div className="flex justify-center items-center h-64">
          <Loader2 className="h-8 w-8 animate-spin text-blue-500" />
          <span className="ml-2 text-gray-500">Loading failures...</span>
        </div>
      ) : (
        <div className="flex-1 relative">
          <FailureTable
            failures={failures}
            onSelect={setSelectedFailure}
            selectedId={selectedFailure?.id || null}
          />
          
          {selectedFailure && (
            <FailureDetail
              failure={selectedFailure}
              onClose={closeDetail}
              onUpdate={handleUpdate}
            />
          )}
        </div>
      )}
    </div>
  );
}
