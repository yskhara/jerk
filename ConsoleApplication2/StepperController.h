#pragma once

#include <fstream>

class Segment
{
public:
	// remaining timestep to be executed for this segment
	int n_step_remaining;

	// number of steps (in total) to be traveled in this segment
	// represented by its absolute value
	// dy in bresenham's algorithm
	int travel_total;
	
	// dx in bresenham's algorithm
	int step_total;

	bool direction;

	int bresenham_error;
};

class StepperController
{
public:
	StepperController(void)
	{
		segment_buffer = new Segment[segment_buffer_size];
		this->ofs = std::ofstream("plot.csv", std::ofstream::trunc);
	}

	void OnInterrupt()
	{
		if (current_segment->n_step_remaining == 0)
		{
			// segment complete
			// try to load next segment
			if (segment_buffer_head != segment_buffer_tail)
			{
				// starving: panic
				return;
			}

			current_segment = &segment_buffer[++segment_buffer_tail];
			segment_buffer_tail &= segment_buffer_mask;

			// bresenham's algorithm: initial value : -dx
			current_segment->bresenham_error = -current_segment->step_total;
		}

		if (current_segment->bresenham_error > 0)
		{
			// step !
			if (current_segment->direction)
			{
				// step forward
				current_position++;
			}
			else
			{
				// step backward
				current_position--;
			}

			// -2*dx
			current_segment->bresenham_error -= 2 * current_segment->step_total;
		}

		// 2*dy
		current_segment->bresenham_error += 2 * current_segment->travel_total;

		current_segment->n_step_remaining--;
	}

	void enqueue_segment(int travel_total, int timestep_total = default_timestep)
	{
		if (((segment_buffer_head - segment_buffer_tail + segment_buffer_size) & segment_buffer_mask) != segment_buffer_mask)
		{
			// queue is not full
			// enqueue new segment
			segment_buffer[segment_buffer_head].travel_total = travel_total;
			segment_buffer[segment_buffer_head].step_total = timestep_total;
		}
	}

private:
	std::ofstream ofs;

	Segment * segment_buffer;
	int segment_buffer_head;
	int segment_buffer_tail;
	static constexpr int segment_buffer_size = 16;
	static constexpr int segment_buffer_mask = segment_buffer_size - 1;
	Segment *current_segment;

	static constexpr int default_timestep = 10;

	int n_steps_segment = 0;


	int current_position;

};
