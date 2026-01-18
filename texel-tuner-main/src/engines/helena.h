#ifndef HELENA_H
#define HELENA_H 1

#define TAPERED 1

#include "../base.h"
#include "../external/chess.hpp"

#include <string>
#include <vector>

namespace Helena
{
    class HelenaEval
    {
    public:
        // Mop-up evaluation is included as non-tunable score (not tuned)
        constexpr static bool includes_additional_score = true;
        constexpr static bool supports_external_chess_eval = true;
        
        // Configuration for the tuner
        constexpr static bool retune_from_zero = false;
        constexpr static tune_t preferred_k = 0; // auto-detect K
        constexpr static int32_t max_epoch = 5001;
        constexpr static bool enable_qsearch = false;
        constexpr static bool filter_in_check = true; // good practice to filter noisy positions
        constexpr static tune_t initial_learning_rate = 2;
        constexpr static int32_t learning_rate_drop_interval = 100;
        constexpr static tune_t learning_rate_drop_ratio = 0.99;
        constexpr static int32_t data_load_print_interval = 10000;

        static parameters_t get_initial_parameters();
        static EvalResult get_fen_eval_result(const std::string& fen);
        static EvalResult get_external_eval_result(const chess::Board& board);
        static void print_parameters(const parameters_t& parameters);
    };
}

#endif // HELENA_H
