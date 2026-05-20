import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { HelmetProvider } from 'react-helmet-async';
import { Toaster } from 'sonner';
import { BookingPage } from '@/pages/BookingPage';
import { ConfirmPage } from '@/pages/ConfirmPage';
import { ViewPage } from '@/pages/ViewPage';
import { CancelPage } from '@/pages/CancelPage';
import { ModifyPage } from '@/pages/ModifyPage';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false,
      retry: 1,
    },
  },
});

function App() {
  return (
    <HelmetProvider>
      <QueryClientProvider client={queryClient}>
        <BrowserRouter>
          <Routes>
            <Route path="/rezervasyon/:slug" element={<BookingPage />} />
            <Route path="/rezervasyon/onay/:code" element={<ConfirmPage />} />
            <Route
              path="/rezervasyon/goruntule/:code"
              element={<ViewPage />}
            />
            <Route path="/rezervasyon/iptal/:code" element={<CancelPage />} />
            <Route
              path="/rezervasyon/degistir/:code"
              element={<ModifyPage />}
            />
            <Route path="*" element={<Navigate to="/rezervasyon/demo-venue" replace />} />
          </Routes>
          <Toaster position="top-center" richColors />
        </BrowserRouter>
      </QueryClientProvider>
    </HelmetProvider>
  );
}

export default App;
