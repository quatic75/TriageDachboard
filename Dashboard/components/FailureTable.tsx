'use client';

import { useState, useEffect } from 'react';
import { Failure } from '../lib/api';
import { ArrowUpDown, AlertCircle, CheckCircle, XCircle } from 'lucide-react';

interface FailureTableProps {
  failures: Failure[];
  onSelect: (failure: Failure) => void;
  selectedId: string | null;
}

export default function FailureTable({ failures: initialFailures, onSelect, selectedId }: FailureTableProps) {
  const [failures, setFailures] = useState<Failure[]>(initialFailures);
  const [sortConfig, setSortConfig] = useState<{ key: keyof Failure; direction: 'asc' | 'desc' } | null>(null);

  useEffect(() => {
    setFailures(initialFailures);
  }, [initialFailures]);

  const handleSort = (key: keyof Failure) => {
    let direction: 'asc' | 'desc' = 'asc';
    if (sortConfig && sortConfig.key === key && sortConfig.direction === 'asc') {
      direction = 'desc';
    }
    setSortConfig({ key, direction });

    const sorted = [...failures].sort((a, b) => {
      const valA = a[key];
      const valB = b[key];

      if (valA === valB) return 0;
      if (valA === undefined || valA === null) return 1;
      if (valB === undefined || valB === null) return -1;

      if (valA < valB) return direction === 'asc' ? -1 : 1;
      if (valA > valB) return direction === 'asc' ? 1 : -1;
      return 0;
    });
    setFailures(sorted);
  };

  const getStatusBadge = (status: string) => {
    switch (status) {
      case 'Open':
        return (
          <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-yellow-100 text-yellow-800">
            <AlertCircle className="w-4 h-4 mr-1" />
            Open
          </span>
        );
      case 'Resolved':
        return (
          <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800">
            <CheckCircle className="w-4 h-4 mr-1" />
            Resolved
          </span>
        );
      case 'NeedsIntervention':
        return (
          <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-red-100 text-red-800">
            <XCircle className="w-4 h-4 mr-1" />
            Needs Intervention
          </span>
        );
      default:
        return <span className="text-gray-500">{status}</span>;
    }
  };

  return (
    <div className="bg-white rounded-lg shadow overflow-hidden">
      <div className="overflow-x-auto">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              {[
                { label: 'Run ID', key: 'runId' },
                { label: 'Pipeline', key: 'pipelineName' },
                { label: 'Status', key: 'status' },
                { label: 'Error', key: 'errorMessage' },
                { label: 'Created', key: 'createdAt' },
              ].map((column) => (
                <th
                  key={column.key}
                  scope="col"
                  className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider cursor-pointer hover:bg-gray-100"
                  onClick={() => handleSort(column.key as keyof Failure)}
                >
                  <div className="flex items-center space-x-1">
                    <span>{column.label}</span>
                    <ArrowUpDown className="w-4 h-4" />
                  </div>
                </th>
              ))}
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200">
            {failures.map((failure) => (
              <tr
                key={failure.id}
                onClick={() => onSelect(failure)}
                className={`cursor-pointer hover:bg-blue-50 transition-colors ${
                  selectedId === failure.id ? 'bg-blue-50 ring-2 ring-inset ring-blue-500' : ''
                }`}
              >
                <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                  {failure.runId.substring(0, 8)}...
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                  {failure.pipelineName}
                </td>
                <td className="px-6 py-4 whitespace-nowrap">
                  {getStatusBadge(failure.status)}
                </td>
                <td className="px-6 py-4 text-sm text-gray-500 max-w-xs truncate" title={failure.errorMessage}>
                  {failure.errorMessage}
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                  {new Date(failure.createdAt).toLocaleString()}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
