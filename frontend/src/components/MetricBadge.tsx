import { Avatar, Box, Paper, Stack, Typography } from '@mui/material';

interface MetricBadgeProps {
  icon: React.ReactNode;
  label: string;
  value: string;
}

const MetricBadge = ({ icon, label, value }: MetricBadgeProps) => (
  <Paper elevation={0} className="metric-badge">
    <Stack direction="row" spacing={1.5} alignItems="center">
      <Avatar variant="rounded" className="metric-icon">
        {icon}
      </Avatar>
      <Box>
        <Typography variant="caption" color="text.secondary">
          {label}
        </Typography>
        <Typography variant="subtitle1">{value}</Typography>
      </Box>
    </Stack>
  </Paper>
);

export default MetricBadge;
